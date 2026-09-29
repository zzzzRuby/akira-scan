from ultralytics import YOLO
import argparse
import torch
import torch.nn as nn
import torch.nn.functional as F
import onnx
import onnxslim
import os

__dirname__ = os.path.dirname(os.path.abspath(__file__))

def train(args):
    model = YOLO('yolo26s.pt')
    model.train(
        data='dataset/data.yaml',
        epochs=150,
        imgsz=640,
        batch=32,
        device='0',
        mosaic=0.5, mixup=0.0,
        patience=20,
        optimizer='AdamW',
        lr0=0.001,
    )

def val(args):
    model = YOLO(os.path.join(__dirname__, '../models/akira-scan.pt'))
    model.val(
        data='dataset/data.yaml',
        epochs=150,
        imgsz=640,
        batch=32,
        device='0',
        mosaic=0.5, mixup=0.0,
        patience=20,
        optimizer='AdamW',
        lr0=0.001,
    )

def infer(args):
    model = YOLO(os.path.join(__dirname__, '../models/akira-scan.pt'))
    results = model.predict("image.jpg", save=True)

class YOLOPreprocess(nn.Module):
    def __init__(self, dtype: torch.dtype, target_size: tuple[int, int]):
        super().__init__()
        self.target_h = int(target_size[0])
        self.target_w = int(target_size[1])
        self.target_dtype = dtype
        self.fill_val = 114.0 / 255.0

    def forward(self, images: torch.Tensor, is_bgra: torch.Tensor):
        images = images.to(self.target_dtype) / 255.0

        indices = torch.where(is_bgra, torch.tensor([2, 1, 0, 3], dtype=torch.long), torch.tensor([0, 1, 2, 3], dtype=torch.long))

        images = torch.index_select(images, dim=3, index=indices)
        images = images[..., :3]
        images = images.permute(0, 3, 1, 2)

        _, _, H, W = images.shape
        
        ratio = torch.min(
            torch.tensor(self.target_w / W, dtype=self.target_dtype),
            torch.tensor(self.target_h / H, dtype=self.target_dtype)
        )

        th_int_tensor = torch.tensor(self.target_h, dtype=torch.int32, device=images.device)
        tw_int_tensor = torch.tensor(self.target_w, dtype=torch.int32, device=images.device)

        new_w = (W * ratio).long()
        new_h = (H * ratio).long()
        
        pad_top = torch.div(th_int_tensor - new_h, 2, rounding_mode='floor')
        pad_left = torch.div(tw_int_tensor - new_w, 2, rounding_mode='floor')

        canvas = torch.full((1, 3, self.target_h, self.target_w), self.fill_val, dtype=images.dtype, device=images.device)
        
        resized = F.interpolate(
            images, 
            size=(new_h.item(), new_w.item()),
            mode='bilinear', 
            align_corners=False
        )
        
        h_slice = torch.narrow(canvas, 2, pad_top.item(), new_h.item())
        target_zone = torch.narrow(h_slice, 3, pad_left.item(), new_w.item())
        
        target_zone.copy_(resized)
        
        return (canvas, pad_top, pad_left, ratio)

class YoloPostprocess(nn.Module):
    def __init__(self):
        super().__init__()

    def forward(self, predictions: torch.Tensor, pad_top: torch.Tensor, pad_left: torch.Tensor, ratio: torch.Tensor):
        predictions[..., [0, 2]] -= pad_left
        predictions[..., [1, 3]] -= pad_top
        predictions[..., [0, 1, 2, 3]] /= ratio

        return predictions

class AkiraScanYolo(nn.Module):
    def __init__(self, yolo_model: nn.Module, dtype: torch.dtype, target_size: tuple[int, int]):
        super().__init__()
        self.preprocess = YOLOPreprocess(dtype, target_size)
        self.yolo = yolo_model
        self.postprocess = YoloPostprocess()

    def forward(self, images: torch.Tensor, is_bgra: torch.Tensor):
        (x_processed, pad_top, pad_left, ratio) = self.preprocess(images, is_bgra)
        results = self.yolo(x_processed)
        return self.postprocess(results[0], pad_top, pad_left, ratio)

def convert(args):
    fp16 = bool(args.fp16)
    onnx_output_path = os.path.join(__dirname__, '../models/akira-scan.fp16.onnx') if fp16 else os.path.join(__dirname__, '../models/akira-scan.onnx')
    dtype = torch.float16 if fp16 else torch.float32
    
    ultralytics_model = YOLO(os.path.join(__dirname__, '../models/akira-scan.pt'))
    pytorch_yolo_model = ultralytics_model.model
    pytorch_yolo_model.eval()

    full_model = AkiraScanYolo(pytorch_yolo_model, dtype, (640, 640))
    full_model.eval()
    if fp16:
        full_model.half()

    dummy_input = (
        torch.randint(0, 255, (1, 640, 640, 4), dtype=torch.uint8),
        torch.tensor(False, dtype=torch.bool),
    )

    torch.onnx.export(
        full_model,
        dummy_input,
        onnx_output_path,
        export_params=True,
        opset_version=18,
        do_constant_folding=True,
        input_names=['images', "is_bgra"],
        output_names=['output'],
        dynamic_shapes=(
            (
                1,
                torch.export.Dim("height"),
                torch.export.Dim("width"),
                4,
            ),
            (),
        ),
        external_data=False,
    )

    model = onnx.load(onnx_output_path)
    slimmed_model = onnxslim.slim(model)
    onnx.save(slimmed_model, onnx_output_path)

def main():
    parser = argparse.ArgumentParser(description="akira-scan train tool.")
    
    subparsers = parser.add_subparsers(dest="command", required=True, help="Available subcommands")

    parser_train = subparsers.add_parser("train")
    parser_train.set_defaults(func=train)

    parser_convert = subparsers.add_parser("convert")
    parser_convert.set_defaults(func=convert)
    parser_convert.add_argument("--fp16", dest="fp16", action='store_true')

    parser_val = subparsers.add_parser("val")
    parser_val.set_defaults(func=val)

    parser_infer = subparsers.add_parser("infer")
    parser_infer.set_defaults(func=infer)

    args = parser.parse_args()
    args.func(args)

if __name__ == "__main__":
    main()
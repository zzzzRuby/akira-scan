import os

# 定义映射规则，例如：原第0类(cat)改为第1类，原第1类(dog)改为第0类
class_mapping = {
    0: 18,
    1: 0,
    2: 9,
    3: 19,
    4: 1,
    5: 10,
    6: 20,
    7: 2,
    8: 11,
    9: 21,
    10: 3,
    11: 12,
    12: 22,
    13: 4,
    14: 13,
    15: 23,
    16: 5,
    17: 14,
    18: 24,
    19: 6,
    20: 15,
    21: 25,
    22: 7,
    23: 16,
    24: 26,
    25: 8,
    26: 17,
    27: 33,
    28: 29,
    29: 27,
    30: 28,
    31: 31,
    32: 32,
    33: 30,
}

labels_dirs = [
    'test/labels',
    'train/labels',
    'valid/labels',
]

for labels_dir in labels_dirs:
    for filename in os.listdir(labels_dir):
        if filename.endswith('.txt'):
            file_path = os.path.join(labels_dir, filename)
            
            with open(file_path, 'r') as file:
                lines = file.readlines()
                
            new_lines = []
            for line in lines:
                parts = line.strip().split()
                if not parts:
                    continue
                
                # 获取原类别索引
                old_class_id = int(parts[0])
                
                # 如果在映射表中，则替换；不在则保留原状（或删除）
                if old_class_id in class_mapping:
                    parts[0] = str(class_mapping[old_class_id])
                    new_line = ' '.join(parts) + '\n'
                    new_lines.append(new_line)
                    
            # 写回修改后的文件
            with open(file_path, 'w') as file:
                file.writelines(new_lines)
const cameras = new Map();

export function register() { }

export async function enumerateDevices() {
    if (!navigator.mediaDevices?.enumerateDevices) {
        return "[]";
    }
    const devices = await navigator.mediaDevices.enumerateDevices();
    const result = devices
        .filter(d => d.kind === 'videoinput')
        .map(d => ({ uniqueId: d.deviceId, label: d.label || 'Camera' }));

    return JSON.stringify(result);
}

export async function startCamera(deviceId, requestedWidth, requestedHeight) {
  const constraints = {
    video: {
      width: requestedWidth || 1920,
      height: requestedHeight || 1080
    }
  };

  if (deviceId) {
    constraints.video.deviceId = { exact: deviceId };
  }
  console.log(constraints)

  const stream = await navigator.mediaDevices.getUserMedia(constraints);

  const video = document.createElement('video');
  video.autoplay = true;
  video.playsInline = true;
  video.muted = true;
  video.srcObject = stream;

  await new Promise((resolve, reject) => {
    video.onloadedmetadata = () => {
      video.play().then(resolve).catch(reject);
    };
    video.onerror = (e) => reject(new Error('Video load error'));
  });

  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d', { willReadFrequently: true });

  const id = Math.random().toString(36).substring(2, 15);
  cameras.set(id, {
    stream,
    video,
    canvas,
    ctx,
    rafId: 0,
    frameCallback: null
  });

  return id;
}

export function getWidth(cameraId) {
  return cameras.get(cameraId)?.video.videoWidth ?? 0;
}

export function getHeight(cameraId) {
  return cameras.get(cameraId)?.video.videoHeight ?? 0;
}

export function readFrame(cameraId) {
  const state = cameras.get(cameraId);
  if (!state || state.video.readyState < 2) {
    return new Uint8Array(0);
  }

  const w = state.video.videoWidth;
  const h = state.video.videoHeight;

  if (state.canvas.width !== w || state.canvas.height !== h) {
    state.canvas.width = w;
    state.canvas.height = h;
  }

  state.ctx.drawImage(state.video, 0, 0);
  const imageData = state.ctx.getImageData(0, 0, w, h);

  return new Uint8Array(imageData.data.buffer, imageData.data.byteOffset, imageData.data.byteLength);
}

export function startFrameLoop(cameraId, callback) {
  const state = cameras.get(cameraId);
  if (!state) return;

  state.frameCallback = callback;

  const loop = () => {
    if (!state.frameCallback) return;
    state.rafId = requestAnimationFrame(loop);
    if (state.video.readyState >= 2) {
      try {
        state.frameCallback();
      } catch (e) {
        console.error('BrowserCamera frame callback error:', e);
      }
    }
  };

  state.rafId = requestAnimationFrame(loop);
}

export function stopFrameLoop(cameraId) {
  const state = cameras.get(cameraId);
  if (state) {
    state.frameCallback = null;
    cancelAnimationFrame(state.rafId);
  }
}

export function stopCamera(cameraId) {
  const state = cameras.get(cameraId);
  if (state) {
    stopFrameLoop(cameraId);
    state.stream.getTracks().forEach(t => t.stop());
    state.video.srcObject = null;
    state.video.remove();
    state.canvas.remove();
    cameras.delete(cameraId);
  }
}
namespace AkiraScan.Camera.GL;

internal interface IGLCameraOutputTextureLifetime
{
    abstract static IGLCameraOutputTextureLifetime? BindToGLTexture(GLContext gl, int width, int height);

    void GLRelease(GLContext gl);
    void Dispose();
}

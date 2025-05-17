using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace App
{
    struct DefinedTriangle
    {
        public Vector2 V1, V2, V3;
        public Vector3 Color;

        public DefinedTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector3 color)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;
        }
    }

    static class Program
    {
        private static int _triangleVao;
        private static int _triangleVbo;
        private static int _shaderProgram;
        private static List<DefinedTriangle> _drawnTriangles = new List<DefinedTriangle>();
        private static List<Vector2> _pendingVertices = new List<Vector2>();
        private static Random _random = new Random();
        
        static void Main()
        {
            GameWindowSettings gameWindowSettings = new GameWindowSettings();
            gameWindowSettings.UpdateFrequency = 60;

            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings();
            nativeWindowSettings.Size = new Vector2i(1280, 720);
            nativeWindowSettings.Title = "TriangulosApp";

            GameWindow gameWindow = new GameWindow(gameWindowSettings, nativeWindowSettings);

            gameWindow.Load += OnLoad;

            gameWindow.MouseDown += (MouseButtonEventArgs args) =>
            {
                if (args.Button == MouseButton.Left && args.Action == InputAction.Press)
                {
                    Vector2 mousePos = gameWindow.MousePosition;
                    
                    float x = (mousePos.X / gameWindow.Size.X) * 2 - 1;
                    float y = -((mousePos.Y / gameWindow.Size.Y) * 2 - 1);
                    Vector2 newVertex = new Vector2(x,y);
                    
                    _pendingVertices.Add(newVertex);

                    if (_pendingVertices.Count == 3)
                    {
                        Vector3 randomColor = new Vector3(
                            (float)_random.NextDouble(),
                            (float)_random.NextDouble(),
                            (float)_random.NextDouble()
                        );
                        
                        _drawnTriangles.Add(new DefinedTriangle(
                            _pendingVertices[0],
                            _pendingVertices[1],
                            _pendingVertices[2],
                            randomColor
                        ));
                        _pendingVertices.Clear();
                    }
                }
            };

            gameWindow.RenderFrame += (FrameEventArgs args) =>
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.UseProgram(_shaderProgram);
                
                GL.BindVertexArray(_triangleVao);

                foreach (var triangle in _drawnTriangles)
                {
                    float[] vertices = {
                        triangle.V1.X, triangle.V1.Y,
                        triangle.V2.X, triangle.V2.Y,
                        triangle.V3.X, triangle.V3.Y
                    };

                    GL.BindBuffer(BufferTarget.ArrayBuffer, _triangleVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

                    int colorLocation = GL.GetUniformLocation(_shaderProgram, "uColor");
                    GL.Uniform3(colorLocation, triangle.Color);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                gameWindow.SwapBuffers();
            };

            gameWindow.Run();
        }

        static void OnLoad()
        {
            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);

            _triangleVbo = GL.GenBuffer();
            _triangleVao = GL.GenVertexArray();

            GL.BindVertexArray(_triangleVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _triangleVbo);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

            string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;
            
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
            }";

            string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            uniform vec3 uColor;
            
            void main()
            {
                FragColor = vec4(uColor, 1.0);
            }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            CheckProgramLinking(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        static void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Shader Compilation Error: {infoLog}");
                throw new Exception($"erro ao compilar o shader: {infoLog}");
            }
        }

        static void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Program Linking Error: {infoLog}");
                throw new Exception($"Program linking error: {infoLog}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimulacaoGrafica
{
    public class Ball
    {
        public PointF Position { get; set; }
        public PointF Velocity { get; set; }
        public float Radius { get; set; }
        public float Mass { get; set; }
        public Color Color { get; set; }
        public float DistanceFromCenter { get; set; }
        public float Angle { get; set; }
    }

    public class SimulationForm : Form
    {
        private List<Ball> balls;
        private Timer timer;
        private Random random;
        private bool isDragging = false;
        private PointF dragOffset;
        private Ball centralBall;


        public SimulationForm()
        {
            balls = new List<Ball>();
            random = new Random();

            GenerateBalls();
            centralBall = balls[0];


            timer = new Timer();
            timer.Interval = 16; // Aproximadamente 60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();

            // Centralizar a simulação na janela
            CenterSimulation();

            // Define o tamanho da janela inicialmente como 1024x768
            ClientSize = new Size(2024, 768);
        }

        private void GenerateBalls()
        {
            Ball superMassiveBall = new Ball
            {
                Position = new PointF(ClientSize.Width / 2, ClientSize.Height / 2),
                Velocity = new PointF(0, 0),
                Radius = 50,
                Mass = 1000,
                Color = Color.Red,
                DistanceFromCenter = 200, // Distância das bolas menores em relação à bola maior
                Angle = 0 // Ângulo inicial das bolas menores
            };

            balls.Add(superMassiveBall);

            ExplodeBall(superMassiveBall);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                float distance = Distance(e.Location, centralBall.Position);
                if (distance <= centralBall.Radius)
                {
                    isDragging = true;
                    dragOffset = new PointF(centralBall.Position.X - e.X, centralBall.Position.Y - e.Y);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isDragging)
            {
                centralBall.Position = new PointF(e.X + dragOffset.X, e.Y + dragOffset.Y);
                Refresh(); // Redesenha a janela para refletir a nova posição da bola
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && isDragging)
            {
                isDragging = false;
            }
        }



        private void CenterSimulation()
        {
            Ball superMassiveBall = balls[0];
            PointF centerOffset = new PointF(ClientSize.Width / 2 - superMassiveBall.Position.X, ClientSize.Height / 2 - superMassiveBall.Position.Y);

            // Aplicar o deslocamento a todas as bolas
            foreach (Ball ball in balls)
            {
                ball.Position = new PointF(ball.Position.X + centerOffset.X, ball.Position.Y + centerOffset.Y);
            }
        }

        private void ExplodeBall(Ball ball)
        {
            balls.Remove(ball);

            float explosionDistance = ball.Radius * 2.5f; // Ajuste o valor multiplicador para aumentar a distância da explosão

            for (int i = 0; i < 10; i++)
            {
                Ball smallBall = new Ball
                {
                    Position = ball.Position,
                    Velocity = new PointF(random.Next(-5, 5), random.Next(-5, 5)),
                    Radius = 10,
                    Mass = random.Next(1, 10),
                    Color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)),
                    DistanceFromCenter = explosionDistance * 2, // Definir a distância da explosão
                    Angle = (float)(2 * Math.PI * i / 10) // Ângulo igualmente espaçado entre as bolas menores
                };

                balls.Add(smallBall);
            }
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateBalls();
            CheckExplosionComplete();
            Invalidate();
        }

        private void UpdateBalls()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                Ball ball = balls[i];
                ball.Position = new PointF(ball.Position.X + ball.Velocity.X, ball.Position.Y + ball.Velocity.Y);

                // Verificar colisões com as bordas da janela
                if (ball.Position.X < ball.Radius)
                {
                    ball.Position = new PointF(ball.Radius, ball.Position.Y);
                    ball.Velocity = new PointF(-ball.Velocity.X, ball.Velocity.Y);
                }
                else if (ball.Position.X > ClientSize.Width - ball.Radius)
                {
                    ball.Position = new PointF(ClientSize.Width - ball.Radius, ball.Position.Y);
                    ball.Velocity = new PointF(-ball.Velocity.X, ball.Velocity.Y);
                }

                if (ball.Position.Y < ball.Radius)
                {
                    ball.Position = new PointF(ball.Position.X, ball.Radius);
                    ball.Velocity = new PointF(ball.Velocity.X, -ball.Velocity.Y);
                }
                else if (ball.Position.Y > ClientSize.Height - ball.Radius)
                {
                    ball.Position = new PointF(ball.Position.X, ClientSize.Height - ball.Radius);
                    ball.Velocity = new PointF(ball.Velocity.X, -ball.Velocity.Y);
                }

                // Aplicar uma leve desaceleração para simular a falta de gravidade
                ball.Velocity = new PointF(ball.Velocity.X * 0.99f, ball.Velocity.Y * 0.99f);

                // Verificar colisões com outras bolas
                for (int j = i + 1; j < balls.Count; j++)
                {
                    Ball otherBall = balls[j];
                    if (CheckCollision(ball, otherBall))
                    {
                        ResolveCollision(ball, otherBall);
                    }
                }

                // Atualizar a posição das bolas menores em relação à bola maior
                if (ball != balls[0])
                {
                    Ball parentBall = balls[0];
                    float x = parentBall.Position.X + ball.DistanceFromCenter * (float)Math.Cos(ball.Angle);
                    float y = parentBall.Position.Y + ball.DistanceFromCenter * (float)Math.Sin(ball.Angle);
                    ball.Position = new PointF(x, y);
                    ball.Angle += 0.05f; // Velocidade de rotação das bolas menores
                }
            }
        }

        private bool CheckCollision(Ball ball1, Ball ball2)
        {
            float distance = Distance(ball1.Position, ball2.Position);
            return distance <= ball1.Radius + ball2.Radius;
        }

        private void ResolveCollision(Ball ball1, Ball ball2)
        {
            // Calcular o vetor de direção entre as bolas
            PointF collisionVector = new PointF(ball2.Position.X - ball1.Position.X, ball2.Position.Y - ball1.Position.Y);

            // Normalizar o vetor de direção
            float magnitude = (float)Math.Sqrt(collisionVector.X * collisionVector.X + collisionVector.Y * collisionVector.Y);
            collisionVector.X /= magnitude;
            collisionVector.Y /= magnitude;

            // Calcular a velocidade relativa das bolas
            PointF relativeVelocity = new PointF(ball2.Velocity.X - ball1.Velocity.X, ball2.Velocity.Y - ball1.Velocity.Y);

            // Calcular a velocidade relativa na direção da colisão
            float collisionSpeed = relativeVelocity.X * collisionVector.X + relativeVelocity.Y * collisionVector.Y;

            // Verificar se as bolas estão se aproximando
            if (collisionSpeed <= 0)
            {
                // Calcular a resposta à colisão com base na conservação do momento e da energia cinética
                float impulse = (2 * collisionSpeed) / (ball1.Mass + ball2.Mass);

                // Atualizar a velocidade das bolas
                ball1.Velocity = new PointF(ball1.Velocity.X + impulse * ball2.Mass * collisionVector.X, ball1.Velocity.Y + impulse * ball2.Mass * collisionVector.Y);
                ball2.Velocity = new PointF(ball2.Velocity.X - impulse * ball1.Mass * collisionVector.X, ball2.Velocity.Y - impulse * ball1.Mass * collisionVector.Y);

                // Aumentar o tamanho da bola1
                float newMass = ball1.Mass + ball2.Mass;
                ball1.Radius = (float)Math.Sqrt(newMass) * 5; // Ajuste o valor "5" para controlar o crescimento da bola

                // Remover a bola2
                balls.Remove(ball2);
            }
        }

        private void CheckExplosionComplete()
        {
            bool allBallsStopped = true;

            foreach (Ball ball in balls)
            {
                if (Math.Abs(ball.Velocity.X) > 0.1f || Math.Abs(ball.Velocity.Y) > 0.1f)
                {
                    allBallsStopped = false;
                    break;
                }
            }

            if (allBallsStopped)
                GenerateNewExplosion();
        }

        private void GenerateNewExplosion()
        {
            Ball randomBall = balls[random.Next(balls.Count)];
            float explosionMultiplier = 1.5f; // Fator de multiplicação do tamanho da explosão
            ExplodeBall(randomBall, explosionMultiplier);
        }

        private void ExplodeBall(Ball ball, float explosionMultiplier)
        {
            balls.Remove(ball);

            float explosionDistance = ball.Radius * 2.5f * explosionMultiplier; // Ajuste o valor multiplicador para aumentar a distância da explosão

            for (int i = 0; i < 10; i++)
            {
                Ball smallBall = new Ball
                {
                    Position = ball.Position,
                    Velocity = new PointF(random.Next(-5, 5), random.Next(-5, 5)),
                    Radius = 10,
                    Mass = random.Next(1, 10),
                    Color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)),
                    DistanceFromCenter = explosionDistance * 2, // Definir a distância da explosão
                    Angle = (float)(2 * Math.PI * i / 10) // Ângulo igualmente espaçado entre as bolas menores
                };

                balls.Add(smallBall);
            }
        }




        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            foreach (Ball ball in balls)
            {
                using (Brush brush = new SolidBrush(ball.Color))
                {
                    float diameter = ball.Radius * 2;
                    RectangleF bounds = new RectangleF(ball.Position.X - ball.Radius, ball.Position.Y - ball.Radius, diameter, diameter);
                    g.FillEllipse(brush, bounds);
                }
            }
        }

        private float Distance(PointF point1, PointF point2)
        {
            float dx = point2.X - point1.X;
            float dy = point2.Y - point1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SimulationForm());
        }
    }
}

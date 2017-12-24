using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Lvl2Lecture1HomeWork
{
    static class Game
    {
        private static Timer _timer = new Timer();
        public static Random Rnd = new Random();
        private static Ship _ship = new Ship(new Point(10, 400), new Point(5, 5), new Size(10, 10));        private static BufferedGraphicsContext _context;
        public static BufferedGraphics Buffer;
        // Свойства
        // Ширина и высота игрового поля
        public static int Width { get; set; }
        public static int Height { get; set; }
        static Game()
        {
        }

        public static void GameLog(string msg)
        {
            Console.WriteLine(msg);
            using (StreamWriter sw = File.AppendText(@"D:\log.txt"))
            {
                sw.WriteLine(msg);
                sw.Close();
            }
        }

        public delegate void GameLogDelegate(string msg);

        private static void Form_KeyDown(object sender, KeyEventArgs e)
        {
            GameLogDelegate msg = new GameLogDelegate(GameLog);
            if (e.KeyCode == Keys.ControlKey) _bullet = new Bullet(new Point(_ship.Rect.X + 10, _ship.Rect.Y + 4),
            new Point(4, 0), new Size(4, 1));
            if (e.KeyCode == Keys.Up) _ship.Up();
            if (e.KeyCode == Keys.Down) _ship.Down();
            if (e.KeyCode == Keys.Space && _ship.CountFirstAidKit > 0)
            {
                _ship.FirstAidKit();
                //GameLog("You used First Aid Kid and recover 15 points of Energy");
                msg.Invoke("You used First Aid Kid and recover 15 points of Energy");
            }
        }

        public static void Init(Form form)
        {
            // Графическое устройство для вывода графики
            Graphics g;
            // предоставляет доступ к главному буферу графического контекста для текущего приложения
            _context = BufferedGraphicsManager.Current;
            g = form.CreateGraphics();// Создаём объект - поверхность рисования и связываем его с формой
                                      // Запоминаем размеры формы
            Width = form.Width;
            Height = form.Height;
            // Связываем буфер в памяти с графическим объектом.
            // для того, чтобы рисовать в буфере
            Buffer = _context.Allocate(g, new Rectangle(0, 0, Width, Height));

            Load();

            //Timer timer = new Timer { Interval = 100 };
            _timer.Start();
            _timer.Tick += Timer_Tick;

            form.KeyDown += Form_KeyDown;
            Ship.MessageDie += Finish;
            GameLogDelegate msg = new GameLogDelegate(GameLog);
            msg.Invoke("New Game!");
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            Draw();
            Update();
        }

        public static void Draw()
        {
            Buffer.Graphics.Clear(Color.Black);
            foreach (BaseObject obj in _objs)
                obj.Draw();
            foreach (Asteroid a in _asteroids)
            {
                a?.Draw();
            }
            _bullet?.Draw();
            _ship?.Draw();
            if (_ship != null)
                Buffer.Graphics.DrawString("Energy:" + _ship.Energy + " FirstAidKit: " + _ship.CountFirstAidKit , SystemFonts.DefaultFont, Brushes.White, 0,
                0);
            Buffer.Render();
        }

        public static BaseObject[] _objs;
        private static Bullet _bullet;
        private static Asteroid[] _asteroids;
        public static void Load()
        {
            _objs = new BaseObject[30];
            _bullet = new Bullet(new Point(0, 200), new Point(5, 0), new Size(4, 1));
            _asteroids = new Asteroid[3];
            var rnd = new Random();
            for (var i = 0; i < _objs.Length; i++)
            {
                int r = rnd.Next(5, 50);
                _objs[i] = new Star(new Point(1000, rnd.Next(0, Game.Height)), new Point(-r, r), new Size(3, 3));
            }
            for (var i = 0; i < _asteroids.Length; i++)
            {
                int r = rnd.Next(5, 50);
                _asteroids[i] = new Asteroid(new Point(1000, rnd.Next(0, Game.Height)), new Point(-r / 5, r), new
                Size(r, r));
            }
        }

        public static void Update()
        {
            GameLogDelegate msg = new GameLogDelegate(GameLog);
            foreach (BaseObject obj in _objs) obj.Update();
            _bullet?.Update();
            for (var i = 0; i < _asteroids.Length; i++)
            {
                if (_asteroids[i] == null) continue;
                _asteroids[i].Update();
                if (_bullet != null && _bullet.Collision(_asteroids[i]))
                {
                    System.Media.SystemSounds.Hand.Play();
                    _asteroids[i] = null;
                    _bullet = null;
                    _ship?.AddScore(100);
                    //GameLog($"You get 100 points of score for destroying Asteroid!");
                    msg.Invoke($"You get 100 points of score for destroying Asteroid!");
                    continue;
                }
                if (!_ship.Collision(_asteroids[i])) continue;
                var rnd = new Random();
                int dmg = rnd.Next(1, 10);
                _ship?.EnergyLow(dmg);
                System.Media.SystemSounds.Asterisk.Play();
                //GameLog($"The Ship was damaged on {dmg} points of Energy");
                msg.Invoke($"The Ship was damaged on {dmg} points of Energy");
                if (_ship.Energy <= 0) _ship?.Die();
            }
        }

        public static void Finish()
        {
            GameLogDelegate msg = new GameLogDelegate(GameLog);
            _timer.Stop();
            Buffer.Graphics.DrawString("The End", new Font(FontFamily.GenericSansSerif, 60,
            FontStyle.Underline), Brushes.White, 200, 100);
            Buffer.Render();
            msg.Invoke("The End");
        }


        class Star : BaseObject
        {
            public Star(Point pos, Point dir, Size size) : base(pos, dir, size)
            {

            }

            public override void Draw()
            {
                Game.Buffer.Graphics.DrawLine(Pens.White, Pos.X, Pos.Y, Pos.X + Size.Width, Pos.Y + Size.Height);
                Game.Buffer.Graphics.DrawLine(Pens.White, Pos.X + Size.Width, Pos.Y, Pos.X, Pos.Y + Size.Height);
            }

            public override void Update()
            {
                Pos.X = Pos.X - Dir.X;
                Pos.Y = Pos.Y - Dir.Y - 2;

                if (Pos.X < 0) Dir.X = -Dir.X;
                if (Pos.X > Game.Width) Dir.X = -Dir.X;
                if (Pos.Y < 0) Dir.Y = -Dir.Y;
                if (Pos.Y > Game.Height) Dir.Y = -Dir.Y;
                //if (Pos.X < 0) Pos.X = Game.Width + Size.Width;
            }


        }

        class Rect : BaseObject
        {
            public Rect(Point pos, Point dir, Size size) : base(pos, dir, size)
            { }

            public override void Draw()
            {
                Rectangle rec = new Rectangle(Pos.X, Pos.Y, Size.Height, Size.Width);
                Game.Buffer.Graphics.DrawRectangle(Pens.White, rec);

            }

            public override void Update()
            {
                Pos.X = Pos.X - Dir.X;
                Pos.Y = Pos.Y - Dir.Y + 2;

                if (Pos.X < 0) Dir.X = -Dir.X;
                if (Pos.X > Game.Width) Dir.X = -Dir.X;
                if (Pos.Y < 0) Dir.Y = -Dir.Y;
                if (Pos.Y > Game.Height) Dir.Y = -Dir.Y;
                //if (Pos.X < 0) Pos.X = Game.Width + Size.Width;
            }
        }
    }
}

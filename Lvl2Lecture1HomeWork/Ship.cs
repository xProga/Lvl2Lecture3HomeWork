using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Lvl2Lecture1HomeWork
{
    class Ship : BaseObject
    {
        public static event Message MessageDie;
        private int _score = 0;
        public int Score => _score;
        private int _countFirstAidKit = 3;
        public int CountFirstAidKit => _countFirstAidKit;
        private int _energy = 100;
        public int Energy => _energy;
        public void EnergyLow(int n)
        {
            _energy -= n;
        }
        public Ship(Point pos, Point dir, Size size) : base(pos, dir, size)
        {
        }
        public override void Draw()
        {
            Game.Buffer.Graphics.FillEllipse(Brushes.Wheat, Pos.X, Pos.Y, Size.Width, Size.Height);
        }
        public override void Update()
        {
        }
        public void Up()
        {
            if (Pos.Y > 0) Pos.Y = Pos.Y - Dir.Y;
        }
        public void Down()
        {
            if (Pos.Y < Game.Height) Pos.Y = Pos.Y + Dir.Y;
        }
        public void FirstAidKit()
        {
            if (_energy == 100)
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
            else
            {
                _countFirstAidKit -= 1;
                _energy += 15;
                if (_energy > 100) _energy = 100;
            }
        }
        public void AddScore(int scoreValue)
        {
            _score += scoreValue;
        }
        public void Die()
        {
            MessageDie?.Invoke();
        }
    }
}

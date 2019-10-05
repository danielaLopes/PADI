using System;

namespace Events
{
    // esta  classe contem os argumentos do evento move do slider
    public class MoveEventArgs : EventArgs
    {
        private int position;
        public MoveEventArgs(int position)
        {
            this.position = position;
        }
        public int Position { get { return position; } }

    }


    class Slider
    {
        private int position;

        // declaração do delegate
        public delegate void Move(object source, MoveEventArgs e);
        // declaração do evento
        public event Move moveEvent;

        public int Position
        {
            get
            {
                return position;
            }
            // e' este bloco que e' executado quando se move o slider
            set
            {
                // call delegate method before setting position
                // validate event is an object of type Move delegate, so it would be null if no class is subscribed
                // to the event so that's why it is necessary to check if it's null before calling a delegate
                if (moveEvent != null)
                    moveEvent(this, new MoveEventArgs(value));

                position = value;
            }
        }
    }


    class Form
    {
        static void Main()
        {
            Slider slider = new Slider();

            // subscribe to moveEvent
            slider.moveEvent += slider_Move;

            // estas sao as duas alteracoes simuladas no slider
            slider.Position = 20;
            slider.Position = 60;
        }

        // este é o método que deve ser chamado quando o slider e' movido
        static void slider_Move(object source, MoveEventArgs e)
        {
            if (e.Position > 50)
                Console.WriteLine("Invalid Position: {0}", e.Position);
            else
                Console.WriteLine("Valid Position: {0}", e.Position);
        }
    }

}

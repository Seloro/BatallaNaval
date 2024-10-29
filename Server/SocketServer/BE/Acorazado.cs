﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Acorazado : BEBarcos
    {
        public int coordenadaX1 { get; set; }
        public int coordenadaX2 { get; set; }
        public int coordenadaX3 { get; set; }

        public int coordenadaY1 { get; set; }
        public int coordenadaY2 { get; set; }
        public int coordenadaY3 { get; set; }

        public Acorazado(bool direccion, int coordenadaX, int coordenadaY)
        {
            Nombre = "Acorazado";
            Slots = 3;
            Id = 1;
            CoordenadaX = coordenadaX;
            CoordenadaY = coordenadaY;
            Horizontal = direccion;
            Vidas = Slots;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using Newtonsoft.Json;

namespace DAL
{
    public class Persistencia
    {
        SqlConnection conexion = new SqlConnection("Data Source=.;Initial Catalog=DatabaseTest;Integrated Security=True");
        private int ID;

        public void insertarDatos(BEBarcos barco)
        {

            SqlCommand insertar = new SqlCommand("INSERT INTO Barcos (Barco_Id, Barco_Tipo, Barco_Direccion, Barco_Vida, Barco_Jugador) VALUES (@pID, @pTipo, @pDireccion, @pVida, @pJugador)", conexion);
            insertar.Parameters.AddWithValue("@pID", generarId("Barco_Id", "Barcos"));
            insertar.Parameters.AddWithValue("@pTipo", barco.Nombre);
            insertar.Parameters.AddWithValue("@pDireccion", barco.Horizontal);
            insertar.Parameters.AddWithValue("@pVida", barco.Vidas);
            insertar.Parameters.AddWithValue("@pJugador", barco.Jugador);

            conexion.Open();
            insertar.ExecuteNonQuery();
            conexion.Close();

            insertarCoordenadas(barco);
        }


        public int generarId(string idTipo, string tabla)
        {
                SqlCommand id = new SqlCommand("SELECT ISNULL(MAX(" + idTipo + "), 0) FROM " + tabla, conexion);
                conexion.Open();
                ID = int.Parse(id.ExecuteScalar().ToString());
                conexion.Close();
            ID++;
            return ID;
        }

        public void eliminarDatos(string tabla)
        {
            SqlCommand eliminar = new SqlCommand("DELETE FROM " + tabla, conexion);

            conexion.Open();
            eliminar.ExecuteNonQuery();
            conexion.Close();
        }

        public void insertarCoordenadas(BEBarcos barco)
        {
            for (int i = 0; i < barco.Vidas; i++)
            {
                SqlCommand insertar = new SqlCommand("INSERT INTO Coordenadas (Coor_Id, Barco_Id, Coor_X, Coor_Y) VALUES (@pCoorID, @pBarcoId, @pCoorX, @pCoorY)", conexion);
                insertar.Parameters.AddWithValue("@pCoorID", generarId("Coor_Id", "Coordenadas"));
                insertar.Parameters.AddWithValue("@pBarcoId", generarId("Barco_Id", "Barcos") - 1);

                insertar.Parameters.AddWithValue("@pCoorX", barco.CoordenadaX[i]);
                insertar.Parameters.AddWithValue("@pCoorY", barco.CoordenadaY[i]);
                conexion.Open();
                insertar.ExecuteNonQuery();
                conexion.Close();

            }
        }
    }
}

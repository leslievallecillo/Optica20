using System;
using System.Configuration;

namespace Optica.Clases
{
    public class Conexion
    {
        public static string CadenaConexion
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace servidor
{
    public class paquete
    {
        public string comando { get; set; }
        public string contenido { get; set; }

        public paquete()
        {
        }

        public paquete(string Com,string Cont)
        {
            comando = Com;
            contenido = Cont;
        }

        public paquete(string datos)
        {
            int sepIndex = datos.IndexOf(":", StringComparison.Ordinal);
            comando = datos.Substring(0,sepIndex);
            contenido = datos.Substring(comando.Length+1);
        }

        public string Serializar()
        {           
            return string.Format("{0}:{1}",comando,contenido);
        }

        public static implicit operator string(paquete paquete)
        {
            return paquete.Serializar();
        }
    }
}

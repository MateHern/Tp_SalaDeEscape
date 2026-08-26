namespace LaMejorSala.Models
{
    public class Acertijo
    {
        public int Id { get; set; }
        public int IdSala { get; set; }
        public int Numero { get; set; }
        public string Pregunta { get; set; }
        public string RespuestaCorrecta { get; set; }
        public string Pista { get; set; }
    }
}
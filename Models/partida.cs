namespace LaMejorSala.Models
{
    public class Partida
    {
        public int Id { get; set; }
        public int IdJugador { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; }
    }
}
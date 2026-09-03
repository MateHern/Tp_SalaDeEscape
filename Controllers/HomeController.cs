using Microsoft.AspNetCore.Mvc;
using LaMejorSala.Models;

namespace LaMejorSala.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Tutorial()
        {
            return View();
        }

        public IActionResult Integrantes()
        {
            return View();
        }

        public IActionResult Identificacion()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Comenzar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ViewBag.Error = "Tenés que escribir tu nombre para comenzar.";
                return View("Identificacion");
            }

            int idJugador = BD.CrearJugador(nombre);

            int idPartida = BD.CrearPartida(idJugador);

            HttpContext.Session.SetInt32("PartidaId", idPartida);
            HttpContext.Session.SetInt32("SalaActual", 1);
            HttpContext.Session.SetString("NombreParticipante", nombre);

            return RedirectToAction("Sala");
        }

        public IActionResult Sala()
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");
            int? salaActual = HttpContext.Session.GetInt32("SalaActual");

            if (partidaId == null || salaActual == null)
            {
                return RedirectToAction("Index");
            }

            Sala sala = BD.ObtenerSala(salaActual.Value);

            if (sala == null)
            {
                return RedirectToAction("Index");
            }

            int ultimaSalaResuelta = BD.ObtenerUltimaSalaResuelta(partidaId.Value);

            if (sala.Numero > ultimaSalaResuelta + 1)
            {
                HttpContext.Session.SetInt32("SalaActual", ultimaSalaResuelta + 1);

                return RedirectToAction("Sala");
            }

            Acertijo acertijo = BD.ObtenerAcertijoActual(
                partidaId.Value,
                sala.Id
            );

            if (acertijo == null)
            {
                BD.MarcarSalaResuelta(partidaId.Value, sala.Id);

                if (sala.Numero == 5)
                {
                    BD.FinalizarPartida(partidaId.Value, "completada");

                    return RedirectToAction("Victoria");
                }

                HttpContext.Session.SetInt32(
                    "SalaActual",
                    sala.Numero + 1
                );

                return RedirectToAction("Sala");
            }

            int errores = BD.ObtenerErrores(partidaId.Value);

            string peligro;
            string mensajePeligro;

            if (errores >= 4)
            {
                peligro = "EXTREMO";
                mensajePeligro = "FABRA ESTÁ CERCA.";
            }
            else if (errores == 3)
            {
                peligro = "MUY PELIGROSO";
                mensajePeligro = "Una voz se escucha cerca: \"No tendrías que estar acá...\"";
            }
            else if (errores == 2)
            {
                peligro = "PELIGROSO";
                mensajePeligro = "Algo golpea una puerta a lo lejos.";
            }
            else if (errores == 1)
            {
                peligro = "SOSPECHOSO";
                mensajePeligro = "Escuchás pasos en algún lugar del pasillo...";
            }
            else
            {
                peligro = "TRANQUILO";
                mensajePeligro = "No escuchás nada. El estadio parece vacío.";
            }

            ViewBag.Sala = sala;
            ViewBag.Acertijo = acertijo;
            ViewBag.Errores = errores;
            ViewBag.Peligro = peligro;
            ViewBag.MensajePeligro = mensajePeligro;

            return View();
        }

        [HttpPost]
        public IActionResult Responder(int idAcertijo, string respuesta)
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");
            int? salaActual = HttpContext.Session.GetInt32("SalaActual");

            if (partidaId == null || salaActual == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(respuesta))
            {
                TempData["Error"] = "Tenés que escribir una respuesta.";
                return RedirectToAction("Sala");
            }

            Sala sala = BD.ObtenerSala(salaActual.Value);

            Acertijo acertijo = BD.ObtenerAcertijo(idAcertijo);

            if (sala == null || acertijo == null)
            {
                return RedirectToAction("Sala");
            }

            string respuestaJugador = respuesta.Trim().ToLower();
            string respuestaCorrecta = acertijo.RespuestaCorrecta.Trim().ToLower();

            bool esCorrecta = respuestaJugador == respuestaCorrecta;

            BD.GuardarRespuesta(
                partidaId.Value,
                sala.Id,
                acertijo.Id,
                respuestaJugador,
                esCorrecta
            );

            if (!esCorrecta)
            {
                int errores = BD.ObtenerErrores(partidaId.Value);

                if (errores >= 5)
                {
                    BD.FinalizarPartida(partidaId.Value, "abortada");

                    return RedirectToAction("FabraAlcanzo");
                }

                TempData["Error"] = "La respuesta es incorrecta. Fabra está cada vez más cerca.";

                return RedirectToAction("Sala");
            }

            TempData["Correcto"] = "Respuesta correcta.";

            int acertijosResueltos =
                BD.CantidadAcertijosResueltos(
                    partidaId.Value,
                    sala.Id
                );

            if (acertijosResueltos >= 4)
            {
                BD.MarcarSalaResuelta(
                    partidaId.Value,
                    sala.Id
                );

                if (sala.Numero == 5)
                {
                    BD.FinalizarPartida(
                        partidaId.Value,
                        "completada"
                    );

                    return RedirectToAction("Victoria");
                }

                HttpContext.Session.SetInt32(
                    "SalaActual",
                    sala.Numero + 1
                );
            }

            return RedirectToAction("Sala");
        }

        public IActionResult Pista(int idAcertijo)
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");

            if (partidaId == null)
            {
                return RedirectToAction("Index");
            }

            Acertijo acertijo = BD.ObtenerAcertijo(idAcertijo);

            if (acertijo == null)
            {
                return RedirectToAction("Sala");
            }

            BD.GuardarPista(
                partidaId.Value,
                idAcertijo
            );

            TempData["Pista"] = acertijo.Pista;

            return RedirectToAction("Sala");
        }

        public IActionResult FabraAlcanzo()
        {
            HttpContext.Session.Clear();

            return View();
        }

        public IActionResult VolverAIntentar()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Identificacion");
        }

        public IActionResult Victoria()
        {
            return View();
        }
    }
}
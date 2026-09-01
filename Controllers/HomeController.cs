using Microsoft.AspNetCore.Mvc;
using LaMejorSala.Models;

namespace LaMejorSala.Controllers
{
    public class HomeController : Controller
    {
        BD bd = new BD();

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
            if (string.IsNullOrEmpty(nombre))
            {
                ViewBag.Error = "Tenés que ingresar tu nombre.";
                return View("Identificacion");
            }

            int idJugador = bd.CrearJugador(nombre);
            int idPartida = bd.CrearPartida(idJugador);

            HttpContext.Session.SetInt32("PartidaId", idPartida);
            HttpContext.Session.SetInt32("SalaActual", 1);
            HttpContext.Session.SetString("NombreParticipante", nombre);

            return RedirectToAction("Sala", new { numero = 1 });
        }

        public IActionResult Sala(int numero)
        {
            int? idPartida = HttpContext.Session.GetInt32("PartidaId");

            if (idPartida == null)
            {
                return RedirectToAction("Identificacion");
            }

            int? salaActual = HttpContext.Session.GetInt32("SalaActual");

            if (salaActual == null)
            {
                salaActual = 1;
            }

            if (numero > salaActual)
            {
                return RedirectToAction("Sala", new { numero = salaActual });
            }

            Sala sala = bd.ObtenerSala(numero);

            if (sala == null)
            {
                return RedirectToAction("Index");
            }

            List<Acertijo> acertijos = bd.ObtenerAcertijos(sala.Id);

            ViewBag.Sala = sala;
            ViewBag.Acertijos = acertijos;
            ViewBag.NumeroSala = numero;

            int errores = bd.ObtenerCantidadErrores(idPartida.Value);

            ViewBag.Errores = errores;
            ViewBag.Peligro = ObtenerPeligro(errores);
            ViewBag.MensajePeligro = ObtenerMensajePeligro(errores);

            return View();
        }

        [HttpPost]
        public IActionResult Responder(int idAcertijo, string respuesta)
        {
            int? idPartida = HttpContext.Session.GetInt32("PartidaId");

            if (idPartida == null)
            {
                return RedirectToAction("Identificacion");
            }

            Acertijo acertijo = bd.ObtenerAcertijo(idAcertijo);

            if (acertijo == null)
            {
                return RedirectToAction("Index");
            }

            bool correcta = false;

            if (!string.IsNullOrEmpty(respuesta))
            {
                correcta = respuesta.Trim().ToLower() ==
                           acertijo.RespuestaCorrecta.Trim().ToLower();
            }

            bd.GuardarRespuesta(
                idPartida.Value,
                acertijo.IdSala,
                acertijo.Id,
                respuesta,
                correcta
            );

            int errores = bd.ObtenerCantidadErrores(idPartida.Value);

            if (errores >= 5)
            {
                bd.FinalizarPartida(idPartida.Value, "ABORTADA");

                return RedirectToAction("FabraAlcanzo");
            }

            if (!correcta)
            {
                TempData["Error"] = "La respuesta no es correcta. Intentá nuevamente.";

                return RedirectToAction(
                    "Sala",
                    new { numero = acertijo.IdSala }
                );
            }


            TempData["Correcto"] = "¡Respuesta correcta!";

            return RedirectToAction(
                "Sala",
                new { numero = acertijo.IdSala }
            );
        }

        public IActionResult Pista(int idAcertijo, int numeroSala)
        {
            int? idPartida = HttpContext.Session.GetInt32("PartidaId");

            if (idPartida == null)
            {
                return RedirectToAction("Identificacion");
            }


            Acertijo acertijo = bd.ObtenerAcertijo(idAcertijo);

            if (acertijo != null)
            {
                bd.GuardarPista(idPartida.Value, idAcertijo);

                TempData["Pista"] = acertijo.Pista;
            }

            return RedirectToAction("Sala", new { numero = numeroSala });
        }

        public IActionResult FabraAlcanzo()
        {
            return View();
        }



        public IActionResult VolverAIntentar()
        {
            string nombre = HttpContext.Session.GetString("NombreParticipante");

            HttpContext.Session.Clear();

            if (nombre != null)
            {
                int idJugador = bd.CrearJugador(nombre);
                int idPartida = bd.CrearPartida(idJugador);


                HttpContext.Session.SetInt32("PartidaId", idPartida);
                HttpContext.Session.SetInt32("SalaActual", 1);
                HttpContext.Session.SetString("NombreParticipante", nombre);

                return RedirectToAction("Sala", new { numero = 1 });
            }

            return RedirectToAction("Identificacion");
        }



        private string ObtenerPeligro(int errores)
        {
            if (errores == 0)
            {
                return "TRANQUILO";
            }


            if (errores == 1)
            {
                return "SOSPECHOSO";
            }




            if (errores == 2)
            {
                return "PELIGROSO";
            }

            if (errores == 3)
            {
                return "MUY PELIGROSO";
            }

            return "EXTREMO";
        }

        private string ObtenerMensajePeligro(int errores)
        {
            if (errores == 0)
            {
                return "No escuchás nada. El estadio parece vacío.";
            }

            if (errores == 1)
            {
                return "Escuchás pasos en algún lugar del pasillo...";
            }


            if (errores == 2)
            {
                return "Algo golpea una puerta a lo lejos.";
            }

            if (errores == 3)
            {
                return "Una voz se escucha cerca: No tendrías que estar acá...";
            }

            return "FABRA ESTÁ MUY CERCA.";
        }
    }
}
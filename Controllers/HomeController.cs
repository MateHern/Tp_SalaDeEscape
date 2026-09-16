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
            HttpContext.Session.SetString(
                "TiempoInicioPartida",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

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

            int errores = BD.ObtenerErrores(partidaId.Value);
            string peligro;
            string mensajePeligro;

            if (sala.Numero == 2)
            {
                int? situacion = HttpContext.Session.GetInt32("SituacionSala2");

                if (situacion == null)
                {
                    situacion = 1;
                    HttpContext.Session.SetInt32("SituacionSala2", 1);
                }

                int erroresSala = BD.ObtenerErrores(partidaId.Value);

                string peligroSala;
                string mensajePeligroSala;

                if (erroresSala >= 4)
                {
                    peligroSala = "EXTREMO";
                    mensajePeligroSala = "FABRA ESTÁ CERCA.";
                }
                else if (erroresSala == 3)
                {
                    peligroSala = "MUY PELIGROSO";
                    mensajePeligroSala = "Una voz se escucha cerca: \"No tendrías que estar acá...\"";
                }
                else if (erroresSala == 2)
                {
                    peligroSala = "PELIGROSO";
                    mensajePeligroSala = "Algo golpea una puerta a lo lejos.";
                }
                else if (erroresSala == 1)
                {
                    peligroSala = "SOSPECHOSO";
                    mensajePeligroSala = "Escuchás pasos en algún lugar del pasillo...";
                }
                else
                {
                    peligroSala = "TRANQUILO";
                    mensajePeligroSala = "No escuchás nada. El estadio parece vacío.";
                }

                ViewBag.Sala = sala;
                ViewBag.Acertijo = acertijo;
                ViewBag.Situacion = situacion;
                ViewBag.Errores = erroresSala;
                ViewBag.Peligro = peligroSala;
                ViewBag.MensajePeligro = mensajePeligroSala;

                return View();
            }

            if (sala.Numero == 1)
            {
                errores = BD.ObtenerErrores(partidaId.Value);

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
                ViewBag.Errores = errores;
                ViewBag.Peligro = peligro;
                ViewBag.MensajePeligro = mensajePeligro;

                return View();
            }

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

            errores = BD.ObtenerErrores(partidaId.Value);

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
        public IActionResult ElegirPuerta(int puerta)
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");
            int? salaActual = HttpContext.Session.GetInt32("SalaActual");

            if (partidaId == null || salaActual == null)
            {
                return RedirectToAction("Index");
            }

            if (salaActual.Value != 2)
            {
                return RedirectToAction("Sala");
            }

            int? situacion = HttpContext.Session.GetInt32("SituacionSala2");

            if (situacion == null)
            {
                situacion = 1;
                HttpContext.Session.SetInt32("SituacionSala2", 1);
            }

            int puertaCorrecta;

            if (situacion == 1)
            {
                puertaCorrecta = 2;
            }
            else if (situacion == 2)
            {
                puertaCorrecta = 1;
            }
            else
            {
                puertaCorrecta = 2;
            }

            Acertijo acertijo = BD.ObtenerAcertijoActual(
                partidaId.Value,
                2
            );

            if (acertijo == null)
            {
                return RedirectToAction("Sala");
            }

            bool esCorrecta = puerta == puertaCorrecta;

            BD.GuardarRespuesta(
                partidaId.Value,
                2,
                acertijo.Id,
                "Puerta " + puerta,
                esCorrecta
            );

            if (!esCorrecta)
            {
                TempData["Error"] = "Elegiste la puerta incorrecta. Tenés que volver a empezar esta sala.";

                HttpContext.Session.SetInt32(
                    "SituacionSala2",
                    1
                );

                return RedirectToAction("Sala");
            }

            if (situacion == 1)
            {
                HttpContext.Session.SetInt32(
                    "SituacionSala2",
                    2
                );

                TempData["Correcto"] = "Correcto. Elegiste la puerta indicada.";

                return RedirectToAction("Sala");
            }

            if (situacion == 2)
            {
                HttpContext.Session.SetInt32(
                    "SituacionSala2",
                    3
                );

                TempData["Correcto"] = "Correcto. Encontraste la segunda puerta.";

                return RedirectToAction("Sala");
            }

            BD.MarcarSalaResuelta(
                partidaId.Value,
                2
            );

            HttpContext.Session.Remove("SituacionSala2");

            HttpContext.Session.SetInt32(
                "SalaActual",
                3
            );

            TempData["Correcto"] = "Lograste atravesar el túnel.";

            return RedirectToAction("Sala");
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

            if (sala.Numero == 4 && acertijo.Numero == 4)
            {
                respuestaCorrecta = "739590";
            }

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
                    return RedirectToAction("Perdiste");
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

        public IActionResult Perdiste()
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

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult RegistrarErrorSala1()
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");

            if (partidaId == null)
            {
                return Json(new { ok = false });
            }

            Acertijo acertijo = BD.ObtenerPrimerAcertijoDeSala(1);

            if (acertijo != null)
            {
                BD.GuardarRespuesta(
                    partidaId.Value,
                    1,
                    acertijo.Id,
                    "Secuencia incorrecta",
                    false
                );
            }

            int errores = BD.ObtenerErrores(partidaId.Value);

            if (errores >= 5)
            {
                BD.FinalizarPartida(partidaId.Value, "abortada");
                return Json(new
                {
                    ok = true,
                    peligroMaximo = true,
                    redirect = Url.Action("FabraAlcanzo")
                });
            }

            return Json(new
            {
                ok = true,
                peligroMaximo = false,
                errores = errores
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult CompletarSala1()
        {
            int? partidaId = HttpContext.Session.GetInt32("PartidaId");

            if (partidaId == null)
            {
                return Json(new { ok = false });
            }

            BD.MarcarSalaResuelta(partidaId.Value, 1);
            HttpContext.Session.SetInt32("SalaActual", 2);

            return Json(new
            {
                ok = true,
                redirect = Url.Action("Sala")
            });
        }
    }
}
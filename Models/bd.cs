using Dapper;
using Microsoft.Data.SqlClient;
using LaMejorSala.Models;

public class BD
{
    private static string connectionString =
    "Server=localhost;Database=EscapeBombonera;Trusted_Connection=True;TrustServerCertificate=True;";

    public static int CrearJugador(string nombre)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                INSERT INTO Jugador (Nombre)
                VALUES (@Nombre);

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return connection.QuerySingle<int>(sql, new
            {
                Nombre = nombre
            });
        }
    }

    public static int CrearPartida(int idJugador)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                INSERT INTO Partida
                (IdJugador, FechaInicio, Estado)
                VALUES
                (@IdJugador, GETDATE(), 'en progreso');

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return connection.QuerySingle<int>(sql, new
            {
                IdJugador = idJugador
            });
        }
    }

    public static Sala ObtenerSala(int numero)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT *
                FROM Sala
                WHERE Numero = @Numero
            ";

            return connection.QueryFirstOrDefault<Sala>(sql, new
            {
                Numero = numero
            });
        }
    }

    public static Acertijo ObtenerAcertijoActual(int idPartida, int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT TOP 1 a.*
                FROM Acertijo a
                WHERE a.IdSala = @IdSala
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM Respuesta r
                    WHERE r.IdPartida = @IdPartida
                    AND r.IdAcertijo = a.Id
                    AND r.EsCorrecta = 1
                )
                ORDER BY a.Numero
            ";

            return connection.QueryFirstOrDefault<Acertijo>(sql, new
            {
                IdPartida = idPartida,
                IdSala = idSala
            });
        }
    }

    public static void GuardarRespuesta(
        int idPartida,
        int idSala,
        int idAcertijo,
        string respuesta,
        bool esCorrecta)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                INSERT INTO Respuesta
                (
                    IdPartida,
                    IdSala,
                    IdAcertijo,
                    RespuestaJugador,
                    EsCorrecta,
                    Fecha
                )
                VALUES
                (
                    @IdPartida,
                    @IdSala,
                    @IdAcertijo,
                    @RespuestaJugador,
                    @EsCorrecta,
                    GETDATE()
                )
            ";

            connection.Execute(sql, new
            {
                IdPartida = idPartida,
                IdSala = idSala,
                IdAcertijo = idAcertijo,
                RespuestaJugador = respuesta,
                EsCorrecta = esCorrecta
            });
        }
    }

    public static int ObtenerErrores(int idPartida)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Respuesta
                WHERE IdPartida = @IdPartida
                AND EsCorrecta = 0
            ";

            return connection.QuerySingle<int>(sql, new
            {
                IdPartida = idPartida
            });
        }
    }

    public static void MarcarSalaResuelta(int idPartida, int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                INSERT INTO ProgresoPartida
                (IdPartida, IdSala, Resuelta)
                VALUES
                (@IdPartida, @IdSala, 1)
            ";

            connection.Execute(sql, new
            {
                IdPartida = idPartida,
                IdSala = idSala
            });
        }
    }

    public static bool SalaResuelta(int idPartida, int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(*)
                FROM ProgresoPartida
                WHERE IdPartida = @IdPartida
                AND IdSala = @IdSala
                AND Resuelta = 1
            ";

            int cantidad = connection.QuerySingle<int>(sql, new
            {
                IdPartida = idPartida,
                IdSala = idSala
            });

            return cantidad > 0;
        }
    }

    public static int CantidadAcertijosResueltos(int idPartida, int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(DISTINCT IdAcertijo)
                FROM Respuesta
                WHERE IdPartida = @IdPartida
                AND IdSala = @IdSala
                AND EsCorrecta = 1
            ";

            return connection.QuerySingle<int>(sql, new
            {
                IdPartida = idPartida,
                IdSala = idSala
            });
        }
    }

    public static void GuardarPista(int idPartida, int idAcertijo)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                INSERT INTO PistaUsada
                (IdPartida, IdAcertijo, Fecha)
                VALUES
                (@IdPartida, @IdAcertijo, GETDATE())
            ";

            connection.Execute(sql, new
            {
                IdPartida = idPartida,
                IdAcertijo = idAcertijo
            });
        }
    }

    public static Acertijo ObtenerAcertijo(int idAcertijo)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT *
                FROM Acertijo
                WHERE Id = @Id
            ";

            return connection.QueryFirstOrDefault<Acertijo>(sql, new
            {
                Id = idAcertijo
            });
        }
    }

    public static Partida ObtenerPartida(int idPartida)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT *
                FROM Partida
                WHERE Id = @Id
            ";

            return connection.QueryFirstOrDefault<Partida>(sql, new
            {
                Id = idPartida
            });
        }
    }

    public static void FinalizarPartida(int idPartida, string estado)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                UPDATE Partida
                SET FechaFin = GETDATE(),
                    Estado = @Estado
                WHERE Id = @IdPartida
            ";

            connection.Execute(sql, new
            {
                IdPartida = idPartida,
                Estado = estado
            });
        }
    }

    public static int ObtenerUltimaSalaResuelta(int idPartida)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT ISNULL(MAX(s.Numero), 0)
                FROM ProgresoPartida p
                INNER JOIN Sala s ON s.Id = p.IdSala
                WHERE p.IdPartida = @IdPartida
                AND p.Resuelta = 1
            ";

            return connection.QuerySingle<int>(sql, new
            {
                IdPartida = idPartida
            });
        }
    }

    public static Acertijo ObtenerPrimerAcertijoDeSala(int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT TOP 1 *
                FROM Acertijo
                WHERE IdSala = @IdSala
                ORDER BY Numero
            ";

            return connection.QueryFirstOrDefault<Acertijo>(sql, new
            {
                IdSala = idSala
            });
        }
    }

    public static int CantidadAcertijosSala(int idSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Acertijo
                WHERE IdSala = @IdSala
            ";

            return connection.QuerySingle<int>(sql, new
            {
                IdSala = idSala
            });
        }
    }

    public static void AsegurarAcertijosSala3()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string idSalaSql = @"
                SELECT Id
                FROM Sala
                WHERE Numero = 3
            ";

            int? idSala = connection.QuerySingleOrDefault<int?>(idSalaSql);

            if (idSala == null)
            {
                return;
            }

            var preguntas = new[]
            {
                new
                {
                    Numero = 1,
                    Pregunta = "¿Qué club uruguayo fue el rival de Boca Juniors en su primer partido internacional de la historia en el año 1907?",
                    Respuesta = "Universal Football Club",
                    Pista = "Su nombre suena muy 'espacial' o del 'cosmos', y el partido terminó con derrota xeneize por 2 a 1 en Buenos Aires."
                },
                new
                {
                    Numero = 2,
                    Pregunta = "¿Quién es la persona con más títulos ganados en la historia del club contando su etapa como jugador y como director técnico?",
                    Respuesta = "Sebastián Battaglia",
                    Pista = "Es un mediocampista central histórico de la época dorada de Carlos Bianchi; de hecho, metió el penal decisivo en Japón contra el Milan en 2003."
                },
                new
                {
                    Numero = 3,
                    Pregunta = "¿Cuál fue el único director técnico brasileño que dirigió al club en la era del profesionalismo?",
                    Respuesta = "Dino Sani",
                    Pista = "Dirigió en 1984, se llamaba Dino y su apellido empieza con S."
                },
                new
                {
                    Numero = 4,
                    Pregunta = "¿A qué equipo mexicano le ganó Boca la final de la Copa Libertadores 2001 para convertirse en bicampeón de América?",
                    Respuesta = "Cruz Azul",
                    Pista = "Es un equipo que viste de azul y blanco, y la final de vuelta se definió por penales en La Bombonera."
                },
                new
                {
                    Numero = 5,
                    Pregunta = "¿Qué histórico delantero xeneize de la década de 1930 tenía el curioso apodo de 'El Expreso de Guayaquil'?",
                    Respuesta = "Francisco Lanz",
                    Pista = "Nacido en Ecuador, hizo una dupla letal con Roberto Cherro y su apellido es Francisco Lanz..."
                },
                new
                {
                    Numero = 6,
                    Pregunta = "¿Quién fue el arquero titular de Boca en la histórica final de la Copa Intercontinental del año 2000 contra el Real Madrid?",
                    Respuesta = "Óscar Córdoba",
                    Pista = "No fue el Pato Abbondanzieri; era un arquero colombiano muy famoso por sus reflejos y por sus pantalones largos."
                },
                new
                {
                    Numero = 7,
                    Pregunta = "¿En qué año se utilizó por primera vez la camiseta azul con la franja amarilla horizontal en el medio?",
                    Respuesta = "1913",
                    Pista = "Fue a principios de la década de 1910; el club adoptó los colores definitivos inspirados en la bandera de un barco sueco."
                }
            };

            foreach (var pregunta in preguntas)
            {
                string updateSql = @"
                    UPDATE Acertijo
                    SET Pregunta = @Pregunta,
                        RespuestaCorrecta = @Respuesta,
                        Pista = @Pista
                    WHERE IdSala = @IdSala
                    AND Numero = @Numero
                ";

                int filasActualizadas = connection.Execute(updateSql, new
                {
                    IdSala = idSala.Value,
                    Numero = pregunta.Numero,
                    Pregunta = pregunta.Pregunta,
                    Respuesta = pregunta.Respuesta,
                    Pista = pregunta.Pista
                });

                if (filasActualizadas == 0)
                {
                    string insertSql = @"
                        INSERT INTO Acertijo (IdSala, Numero, Pregunta, RespuestaCorrecta, Pista)
                        VALUES (@IdSala, @Numero, @Pregunta, @Respuesta, @Pista)
                    ";

                    connection.Execute(insertSql, new
                    {
                        IdSala = idSala.Value,
                        Numero = pregunta.Numero,
                        Pregunta = pregunta.Pregunta,
                        Respuesta = pregunta.Respuesta,
                        Pista = pregunta.Pista
                    });
                }
            }
        }
    }
}
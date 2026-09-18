using Dapper;
using Microsoft.Data.SqlClient;
using LaMejorSala.Models;

public class BD
{
    private static string connectionString =
    "Server=localhost;Database=EscapeBombonera;Trusted_Connection=True;TrustServerCertificate=True;";

    public static Acertijo ObtenerAcertijoPorSalaYNumero(int idSala, int numero)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string sql = @"
                SELECT TOP 1 *
                FROM Acertijo
                WHERE IdSala = @IdSala
                AND Numero = @Numero
            ";

            return connection.QueryFirstOrDefault<Acertijo>(sql, new
            {
                IdSala = idSala,
                Numero = numero
            });
        }
    }

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
        int? idAcertijo,
        string respuesta,
        bool esCorrecta)
    {
        if (!idAcertijo.HasValue)
        {
            return;
        }

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
                    Pregunta = "En una bandera encontrás las letras B O C A. Debajo hay cuatro números: 2 - 15 - 3 - 1. Pero esta vez tenés que multiplicar el valor de la primera letra por el de la última y sumar los valores de las dos letras del medio. ¿Cuál es el resultado?",
                    Respuesta = "20",
                    Pista = "Usá el valor de cada letra del abecedario. Primero multiplicá y después sumá."
                },
                new
                {
                    Numero = 2,
                    Pregunta = "Escuchás a la barra cantar y encontrás una pared con esta secuencia: 1 - 4 - 9 - 16 - 25 - ?. ¿Qué número falta?",
                    Respuesta = "36",
                    Pista = "Cada número es el resultado de multiplicar un número por sí mismo."
                },
                new
                {
                    Numero = 3,
                    Pregunta = "Encontrás cinco escalones numerados. El primero tiene 3, el segundo 6, el tercero 12 y el cuarto 24. En el quinto alguien escribió solamente un signo de pregunta. ¿Qué número debería aparecer?",
                    Respuesta = "48",
                    Pista = "Cada escalón duplica el número anterior."
                },
                new
                {
                    Numero = 4,
                    Pregunta = "Una puerta tiene tres candados. Cada uno tiene un número: 4, 7 y 12. Una nota dice: El primer número aumenta 3 y el segundo aumenta 5. Seguí la misma lógica. ¿Cuál sería el siguiente número?",
                    Respuesta = "19",
                    Pista = "Las diferencias entre los números también esconden una secuencia."
                },
                new
                {
                    Numero = 5,
                    Pregunta = "¿Qué club uruguayo fue el rival de Boca Juniors en su primer partido internacional de la historia en el año 1907?",
                    Respuesta = "Universal De Montevideo",
                    Pista = "Su nombre suena muy 'espacial' o del 'cosmos', y el partido terminó con derrota xeneize por 2 a 1 en Buenos Aires."
                },
                new
                {
                    Numero = 6,
                    Pregunta = "¿Quién es la persona con más títulos ganados en la historia del club contando su etapa como jugador y como director técnico?",
                    Respuesta = "Sebastián Battaglia",
                    Pista = "Es un mediocampista central histórico de la época dorada de Carlos Bianchi; de hecho, metió el penal decisivo en Japón contra el Milan en 2003."
                },
                new
                {
                    Numero = 7,
                    Pregunta = "¿Cuál fue el único director técnico brasileño que dirigió al club en la era del profesionalismo?",
                    Respuesta = "Dino Sani",
                    Pista = "Dirigió en 1984, se llamaba Dino y su apellido empieza con S."
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
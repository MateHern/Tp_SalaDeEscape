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
}
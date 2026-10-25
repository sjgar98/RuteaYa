namespace RuteaYa.DTOs.Enums
{
    public enum DiaSemana : byte
    {
        Lunes = 1,
        Martes = 2,
        Miercoles = 3,
        Jueves = 4,
        Viernes = 5,
        Sabado = 6,
        Domingo = 7
    }

    public static class DiaSemanaExtensions
    {
        public static DiaSemana ADiaSemana(this DayOfWeek dia)
        {
            return dia == DayOfWeek.Sunday ? DiaSemana.Domingo : (DiaSemana)(byte)dia;
        }

        public static DiaSemana ADiaSemana(this DateOnly fecha)
        {
            return fecha.DayOfWeek.ADiaSemana();
        }
    }
}

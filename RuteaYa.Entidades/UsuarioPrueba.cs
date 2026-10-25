namespace RuteaYa.Entidades
{
    // Usuario sembrado por migración para probar la aplicación desde una base vacía:
    // se puede iniciar sesión con Email y Password.
    public static class UsuarioPrueba
    {
        public const int Id = 1;
        public const string Nombre = "Usuario";
        public const string Apellido = "Prueba";
        public const string Email = "prueba@ruteaya.com";
        public const string Password = "Prueba123";

        // Hash de Password con PasswordHasher (formato V3, PBKDF2). Es fijo porque HasData compara el
        // seed entre migraciones; un test verifica que corresponda a Password.
        public const string PasswordHash = "AQAAAAIAAYagAAAAEKndklYBED8heEK5UUKWAFP0VZIhX94RvKi/4waoLwHrspyn1PcxGIq8VsiZOkk/NQ==";
    }
}

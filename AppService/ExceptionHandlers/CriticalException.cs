namespace AppService.ExceptionHandlers;

public class CriticalException(string message) : Exception(message);


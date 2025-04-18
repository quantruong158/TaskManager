namespace TaskManager.Api.Exceptions
{
    public class AppException : Exception
    {
        public AppException() : base() {}
        
        public AppException(string message) : base(message) {}
        
        public AppException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class NotFoundException : AppException
    {
        public NotFoundException() : base() {}
        
        public NotFoundException(string message) : base(message) {}
        
        public NotFoundException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class BadRequestException : AppException
    {
        public BadRequestException() : base() {}
        
        public BadRequestException(string message) : base(message) {}
        
        public BadRequestException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException() : base() {}
        
        public UnauthorizedException(string message) : base(message) {}
        
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException() : base() {}
        
        public ForbiddenException(string message) : base(message) {}
        
        public ForbiddenException(string message, Exception innerException) : base(message, innerException) {}
    }
}
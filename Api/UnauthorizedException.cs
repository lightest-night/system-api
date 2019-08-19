using System;

namespace LightestNight.System.Api
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Unauthorized")
        {}
    }
}
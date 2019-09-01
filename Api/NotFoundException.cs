using System;

namespace LightestNight.System.Api
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Not Found")
        {}
    }
}
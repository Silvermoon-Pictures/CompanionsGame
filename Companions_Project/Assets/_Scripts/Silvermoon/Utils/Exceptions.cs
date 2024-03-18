using System;
using UnityEngine;

namespace Silvermoon.Utils
{
    public class DesignException : Exception
    {
        public DesignException(string message) : base(message) { }
    }
}
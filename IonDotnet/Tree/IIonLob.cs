﻿using System;
namespace IonDotnet.Tree
{
    public interface IIonLob
    {
        ReadOnlySpan<byte> Bytes();
        void SetBytes(ReadOnlySpan<byte> buffer);
        IonType Type { get; }
        bool IsNull { get; }
    }
}

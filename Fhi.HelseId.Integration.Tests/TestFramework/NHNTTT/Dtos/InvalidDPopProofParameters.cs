using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT.Dtos
{
    internal enum InvalidDPoPProofParameters
    {

        [System.Runtime.Serialization.EnumMember(Value = @"0")]
        _0___None = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"1")]
        _1___DontSetHtuClaimValue = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"2")]
        _2___DontSetHtmClaimValue = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"3")]
        _3___SetIatValueInThePast = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"4")]
        _4___SetIatValueInTheFuture = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"5")]
        _5___DontSetAthClaimValue = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"6")]
        _6___DontSetAlgHeader = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"7")]
        _7___DontSetJwkHeader = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"8")]
        _8___DontSetJtiClaim = 8,

        [System.Runtime.Serialization.EnumMember(Value = @"9")]
        _9___SetAlgHeaderToASymmetricAlgorithm = 9,

        [System.Runtime.Serialization.EnumMember(Value = @"10")]
        _10___SetPrivateKeyInJwkHeader = 10,

        [System.Runtime.Serialization.EnumMember(Value = @"11")]
        _11___SetInvalidTypHeaderValue = 11,

        [System.Runtime.Serialization.EnumMember(Value = @"12")]
        _12___SetAnInvalidSignature = 12,

    }

}

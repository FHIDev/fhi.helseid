using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT.Dtos
{
    internal enum ParametersGeneration
    {
        GenerateNone,
        GenerateOnlyDefault,
        GenerateOnlyFromNonEmptyParameterValues,
        GenerateDefaultWithClaimsFromNonEmptyParameterValues
    }
}

// Copyright © 2020 ONIXLabs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Modifications were made in this project to the original ONIXLabs source code to comply with the latest
// .NET design guidelines: https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
// Additional modifications were made to only include the SHA-3 256-bit hash algorithm.

namespace OnixLabs.Security.Cryptography;

public abstract partial class Sha3
{
    /// <summary>
    /// Creates an instance of the <see cref="Sha3Hash256"/> algorithm.
    /// </summary>
    /// <returns>An instance of the <see cref="Sha3Hash256"/> algorithm.</returns>
    public static Sha3 CreateSha3Hash256()
    {
        return new Sha3Hash256();
    }
}

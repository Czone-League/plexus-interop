/**
 * Copyright 2017-2018 Plexus Interop Deutsche Bank AG
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
﻿namespace Plexus.Interop.Metamodel.Json.Internal
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal sealed class ProvidedMethodDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("options")]
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }
}

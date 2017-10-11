/**
 * Copyright 2017 Plexus Interop Deutsche Bank AG
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
import * as log from "loglevel";
import {Logger} from "./Logger";
import {LoggerBase} from "./LoggerBase";
const logPrefixer: any = require("loglevel-plugin-prefix");

export enum LogLevel {
  TRACE,
  DEBUG,
  INFO,
  WARN,
  ERROR,
  SILENT
}

export class LoggerFactory {

    public static getLogger(name: string = "Anonymous"): Logger {
        return new LoggerBase(name);
    }

    public static setLogLevel(level: LogLevel): void {
      log.setLevel(level as any);
    }

}

logPrefixer.apply(log, {
  template: "%t | [%l] ",
  timestampFormatter: function (date: Date) {
    return `${date.toTimeString().replace(/.*(\d{2}:\d{2}:\d{2}).*/, '$1')}.${('000' + date.getMilliseconds()).slice(-3)}`;
  }
});

LoggerFactory.setLogLevel(LogLevel.INFO);
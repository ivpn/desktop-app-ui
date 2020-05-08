//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
using System.Text;
using System.IO;

namespace IVPN
{
    public class FileUtils
    {
        public const int MaxLogSize = 1024 * 64;
        public static string TailOfLog(string fileName, int tailLength = MaxLogSize)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var reader = new StreamReader(fs))
                {
                    if (reader.BaseStream.Length > tailLength)
                    {
                        reader.BaseStream.Seek(-tailLength, SeekOrigin.End);
                    }

                    string line;
                    while ((line = reader.ReadLine()) != null)                    
                        builder.AppendLine(line);
                }

                return builder.ToString();

            } 
            catch (FileNotFoundException)
            {
                return $"File '{Path.GetFileName(fileName)}' not exists";
            }
            catch (Exception e)
            {
                return $"Error occured when reading from file {Path.GetFileName(fileName)}:\n{e}";
            }
        }
    }
}


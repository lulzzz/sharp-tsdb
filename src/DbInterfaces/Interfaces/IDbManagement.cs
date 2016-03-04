﻿// /*******************************************************************************
//  * Copyright (c) 2016 by RF77 (https://github.com/RF77)
//  * All rights reserved. This program and the accompanying materials
//  * are made available under the terms of the Eclipse Public License v1.0
//  * which accompanies this distribution, and is available at
//  * http://www.eclipse.org/legal/epl-v10.html
//  *
//  * Contributors:
//  *    RF77 - initial API and implementation and/or initial documentation
//  *******************************************************************************/ 

using System.Collections.Generic;

namespace DbInterfaces.Interfaces
{
    public interface IDbManagement
    {
        IDb CreateDb(string directoryPath, string name);
        IDb GetDb(string name);
        IReadOnlyList<string> GetDbNames();
        void DeleteDb(string name);
        void AttachDb(string dbPath);
        void DetachDb(string dbName);
        void DetachAllDbs();
        IDb GetOrCreateDb(string directoryPath, string name);
    }
}
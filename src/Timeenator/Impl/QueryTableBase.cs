// /*******************************************************************************
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
using Timeenator.Interfaces;

namespace Timeenator.Impl
{
    public abstract class QueryTableBase<T> : IQueryTableBase<T> where T : struct
    {
        public abstract IObjectQuerySerieBase TryGetSerie(string name);
        public abstract IObjectQuerySerieBase GetOrCreateSerie(string name);

        public IEnumerable<IObjectQuerySerie> Series => GetSeries();
        protected abstract IEnumerable<IObjectQuerySerie> GetSeries();
    }
}
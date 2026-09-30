// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Data.Models;

namespace Workflow.Models;

public sealed class AppConfiguration
{
    public CoreDataConfiguration CoreData { get; set; }
}
// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Models;

public class Result<T> : Result
{
    private string id;

    [Key]
    public override string Id
    {
        get
        {
            if (id != null)
            {
                return id;
            }

            try
            {
                return Item is null
                    ? null
                    : ((dynamic)Item).Id?.ToString();
            }
            catch
            {
                return null;
            }
        }
        set => id = value;
    }

    public T Item { get; set; }
}
using System;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PagedList<T> : List<T>
{
    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count/(double)pageSize);
        PageSize = pageSize;
        TotalCount = count;
        AddRange(items);

    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize) // <T> to make it generic to be used by any entites. //and we can swap it for any other types.
    {
        var count = await source.CountAsync(); // to check how many items we have in the database beofre pagination.
        var items = await source
                        .Skip((pageNumber - 1 ) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize); //Creates and returns an instance of PagedList<T>


    }

}
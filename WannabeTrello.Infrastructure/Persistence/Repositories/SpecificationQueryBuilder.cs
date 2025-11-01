﻿using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class SpecificationQueryBuilder
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery, 
        ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        // Apply no tracking if specified
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
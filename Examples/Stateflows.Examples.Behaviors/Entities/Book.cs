using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Examples.Behaviors.Entities.Events;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Examples.Behaviors.Entities;

public interface Book
{
    [Initializer]
    void Initialize(InitializeBook initializeBook)
    {
        Author = initializeBook.Author;
        Title = initializeBook.Title;
        ISBN = initializeBook.ISBN;
    }
    
    [Required, MinLength(8), Field(FieldAccess.Set | FieldAccess.Get)] string? Author { get; [NoApiMapping] set; }
    [Field(FieldAccess.Set)] string? Title { get; set; }
    [Field(FieldAccess.Get)] string? ISBN { get; set; }
    [Field] string? Comment { get; set; }
    [Projection(PublishScope.Self)] string DisplayName => $"{Title} by {Author}";
    [Mutation] void UpdateTitle(string newTitle) => Title = newTitle;
}
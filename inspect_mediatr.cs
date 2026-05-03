using System;
using System.Linq;
using MediatR;
foreach (var type in new[] { typeof(IMediator), typeof(ISender), typeof(IPublisher) })
{
    Console.WriteLine(type.FullName);
    foreach (var method in type.GetMethods())
    {
        Console.WriteLine(method);
    }
}

using Bogus;
using FileApp.FileCreator.Core.Abstraction;
using FileApp.FileCreator.Core.Models;

namespace FileApp.FileCreator.Core.DataGenerator;

public class FakeDataGenerator : IDataGenerator
{
    private readonly Faker<FileLine> _faker;

    public FakeDataGenerator() : this(CreateFileLineFaker())
    {
    }

    public FakeDataGenerator(Faker<FileLine> faker)
    {
        _faker = faker;
    }

    public FileLine GenerateData()
    {
        var fileLine = _faker.Generate();

        return fileLine;
    }

    private static Faker<FileLine> CreateFileLineFaker()
    {
        var faker = new Faker<FileLine>()
            .RuleFor(v => v.Number, f => f.Random.Long(1, long.MaxValue))
            .RuleFor(v => v.Content, f => f.Lorem.Sentence(1, 2).TrimEnd('.'));

        return faker;
    }
}
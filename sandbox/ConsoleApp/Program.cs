using MasterMemory;
using System.Linq;
using MessagePack;
using System;
using System.IO;
using System.Buffers;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

// IValidatable����������ƌ��ؑΏۂɂȂ�
[MemoryTable("quest_master"), MessagePackObject(true)]
public class Quest : IValidatable<Quest>
{
    // UniqueKey�̏ꍇ��Validate���Ƀf�t�H���g�ŏd�����̌��؂������
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public int RewardId { get; set; }
    public int Cost { get; set; }

    void IValidatable<Quest>.Validate(IValidator<Quest> validator)
    {
        // �O���L�[�I�ɎQ�Ƃ������R���N�V���������o����
        var items = validator.GetReferenceSet<Item>();

        // RewardId��0�ȏ�̂Ƃ�(0�͕�V�i�V�̂��߂̓��ʂȃt���O�Ƃ��邽�ߓ��͂����e����)
        if (this.RewardId > 0)
        {
            // Items�̃}�X�^�ɕK���܂܂�ĂȂ���Ό��؃G���[�i�G���[���o�Ă����s�͂��Ă��ׂĂ̌��،��ʂ��o��)
            items.Exists(x => x.RewardId, x => x.ItemId);
        }

        // �R�X�g��10..20�łȂ���Ό��؃G���[
        validator.Validate(x => x.Cost >= 10);
        validator.Validate(x => x.Cost <= 20);

        // �ȉ��ň͂��������͈�x�����Ă΂�Ȃ����߁A�f�[�^�Z�b�g�S�̂̌��؂����������Ɏg����
        if (validator.CallOnce())
        {
            var quests = validator.GetTableSet();
            // �C���f�b�N�X�����������̈ȊO�̃��j�[�N�ǂ����̌���(0�͏d�����邽�ߏ����Ă���)
            quests.Where(x => x.RewardId != 0).Unique(x => x.RewardId);
        }
    }
}

[MemoryTable("item"), MessagePackObject(true)]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}

namespace ConsoleApp
{
    [MemoryTable("monster"), MessagePackObject(true)]
    public class Monster
    {
        [PrimaryKey]
        public int MonsterId { get; }
        public string Name { get; }
        public int MaxHp { get; }

        public Monster(int MonsterId, string Name, int MaxHp)
        {
            this.MonsterId = MonsterId;
            this.Name = Name;
            this.MaxHp = MaxHp;
        }
    }




    public enum Gender
    {
        Male, Female
    }

    [MemoryTable("person"), MessagePackObject(true)]
    public class Person
    {
        [PrimaryKey(keyOrder: 1)]
        public int PersonId { get; set; }
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(2, keyOrder: 1), NonUnique]
        public int Age { get; set; }
        [SecondaryKey(1), NonUnique]
        [SecondaryKey(2, keyOrder: 0), NonUnique]
        public Gender Gender { get; set; }
        public string Name { get; set; }

        public Person()
        {
        }

        public Person(int PersonId, int Age, Gender Gender, string Name)
        {
            this.PersonId = PersonId;
            this.Age = Age;
            this.Gender = Gender;
            this.Name = Name;
        }

        public override string ToString()
        {
            return $"{PersonId} {Age} {Gender} {Name}";
        }
    }



    

    class ByteBufferWriter : IBufferWriter<byte>
    {
        byte[] buffer;
        int index;

        public int CurrentOffset => index;
        public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);
        public ReadOnlyMemory<byte> WrittenMemory => new ReadOnlyMemory<byte>(buffer, 0, index);

        public ByteBufferWriter()
        {
            buffer = new byte[1024];
            index = 0;
        }

        public void Advance(int count)
        {
            index += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            AGAIN:
            var nextSize = index + sizeHint;
            if (buffer.Length < nextSize)
            {
                Array.Resize(ref buffer, Math.Max(buffer.Length * 2, nextSize));
            }

            if (sizeHint == 0)
            {
                var result = new Memory<byte>(buffer, index, buffer.Length - index);
                if (result.Length == 0)
                {
                    sizeHint = 1024;
                    goto AGAIN;
                }
                return result;
            }
            else
            {
                return new Memory<byte>(buffer, index, sizeHint);
            }
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetMemory(sizeHint).Span;
        }
    }

    [MemoryTable(nameof(Test1))]
    public class Test1
    {
        [PrimaryKey]
        public int Id { get; set; }
    }

    [MessagePackObject(false)]
    [MemoryTable(nameof(Test2))]
    public class Test2
    {
        [PrimaryKey]
        public int Id { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bin = new DatabaseBuilder().Append(new Monster[]
            {
                new Monster ( MonsterId : 1, Name : "Foo", MaxHp : 100 )
            }).Append(new Person[]
            {
                new Person { PersonId = 0, Age = 13, Gender = Gender.Male,   Name = "Dana Terry" },
                new Person { PersonId = 1, Age = 17, Gender = Gender.Male,   Name = "Kirk Obrien" },
                new Person { PersonId = 2, Age = 31, Gender = Gender.Male,   Name = "Wm Banks" },
                new Person { PersonId = 3, Age = 44, Gender = Gender.Male,   Name = "Karl Benson" },
                new Person { PersonId = 4, Age = 23, Gender = Gender.Male,   Name = "Jared Holland" },
                new Person { PersonId = 5, Age = 27, Gender = Gender.Female, Name = "Jeanne Phelps" },
                new Person { PersonId = 6, Age = 25, Gender = Gender.Female, Name = "Willie Rose" },
                new Person { PersonId = 7, Age = 11, Gender = Gender.Female, Name = "Shari Gutierrez" },
                new Person { PersonId = 8, Age = 63, Gender = Gender.Female, Name = "Lori Wilson" },
                new Person { PersonId = 9, Age = 34, Gender = Gender.Female, Name = "Lena Ramsey" },
            })
            .Append(new Quest[]
            {
                new Quest { Id= 1, Name = "foo", Cost = 10, RewardId = 100 },
                new Quest { Id= 2, Name = "bar", Cost = 20, RewardId = 101 },
                new Quest { Id= 3, Name = "baz", Cost = 30, RewardId = 0 },
                new Quest { Id= 3, Name = "too", Cost = 40, RewardId = 0 },
            })
            .Append(new Item[]
            {
                new Item { ItemId = 100 },
                new Item { ItemId = 101 },
                new Item { ItemId = 199 },
            })
            .Build();




            var db = new MemoryDatabase(bin);


            // �e�[�u�����A�v���p�e�B���A�C���f�b�N�X��񂪎���̂Ŏ��R�ɉ��H����
            var metaDb = MetaMemoryDatabase.GetMetaDatabase();
            foreach (var table in metaDb.GetTableInfos())
            {
                // CSV�̃w�b�_����
                var sb = new StringBuilder();
                foreach (var prop in table.Properties)
                {
                    if (sb.Length != 0) sb.Append(",");

                    // ���̂܂�, LowerCamelCase, SnakeCase�ɕϊ��������O���擾�\
                    sb.Append(prop.NameSnakeCase);
                }
                Console.WriteLine(sb.ToString());
                File.WriteAllText(table.TableName + ".csv", sb.ToString(), new UTF8Encoding(false));
            }



            // ���،��ʎ擾�B�f�[�^�x�[�X�̍\�z���̂͌��؂Ƃ͖��֌W�ɍ\�z���ł���̂ŁA
            // �i�J�����p�ȂǂɁj�s�����̂܂܏o���Ă��������A(�����[�X���ł�)�e���Ȃǂ����R�ɁB
            var validateResult = db.Validate();
            if (validateResult.IsValidationFailed)
            {
                // ���؎��s�f�[�^�𕶎���`���Ńt�H�[�}�b�g���ďo��
                Console.WriteLine(validateResult.FormatFailedResults());

                // List<(Type, string)> �Ō��؃f�[�^���擾���āA�����ŃJ�X�^���ŏo�͂��邱�Ƃ��\
                // MD��HTML�ɐ��`����Slack�⃌�|�[�^�[�ɓ�����Ȃǎ��R��
                // validateResult.FailedResults
            }

            // new MetaMemoryDatabase()

        }
    }
}



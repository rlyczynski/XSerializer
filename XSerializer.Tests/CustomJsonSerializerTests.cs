﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using XSerializer.Encryption;
using XSerializer.Tests.Encryption;

namespace XSerializer.Tests
{
    public class CustomJsonSerializerTests
    {
        [Test]
        public void CanSerializeReadWriteProperties()
        {
            var serializer = new JsonSerializer<Bar>();

            var instance = new Bar
                {
                    Baz = new Baz
                    {
                        Qux = "abc",
                        Garply = true
                    },
                    Corge = 123.45
                };

            var json = serializer.Serialize(instance);

            Assert.That(json, Is.EqualTo(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}"));
        }

        [Test]
        public void CanSerializeWithEncryptRootObjectEnabled()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<Bar>(configuration);

            var instance = new Bar
            {
                Baz = new Baz
                {
                    Qux = "abc",
                    Garply = true
                },
                Corge = 123.45
            };

            var json = serializer.Serialize(instance);

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void AClassDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Grault>(configuration);

            var instance = new Grault
            {
                Qux = "abc",
                Garply = true
            };

            var json = serializer.Serialize(instance);

            var expected =
                @""""
                + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                + @"""";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void APropertyDecoratedWithTheEncryptAttributeIsEncrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Waldo>(configuration);

            var instance = new Waldo
            {
                Qux = "abc",
                Garply = true
            };

            var json = serializer.Serialize(instance);

            var expected =
                @"{""Qux"":"""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @""",""Garply"":"""
                + encryptionMechanism.Encrypt("true")
                + @"""}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void DuplicatedEncryptAttributesHaveNoEffectOnSerialization()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Thud>(configuration);

            var instance = new Thud
            {
                Grault = new Grault
                {
                    Qux = "abc",
                    Garply = true
                },
                Waldo = new Waldo
                {
                    Qux = "abc",
                    Garply = true
                }
            };

            var json = serializer.Serialize(instance);

            var expected =
                @"{""Grault"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @",""Waldo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @"}";

            Assert.That(json, Is.EqualTo(expected));
        }

        [Test]
        public void CanDeserializeReadWriteProperties()
        {
            const string json = @"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}";

            var serializer = new JsonSerializer<Bar>();

            var instance = serializer.Deserialize(json);

            Assert.That(instance.Baz.Qux, Is.EqualTo("abc"));
            Assert.That(instance.Baz.Garply, Is.EqualTo(true));
            Assert.That(instance.Corge, Is.EqualTo(123.45));
        }

        [Test]
        public void CanDeserializeEmptyObject()
        {
            const string json = @"{}";

            var serializer = new JsonSerializer<Bar>();

            var instance = serializer.Deserialize(json);

            Assert.That(instance.Baz, Is.Null);
            Assert.That(instance.Corge, Is.EqualTo(0));
        }

        [Test]
        public void CanDeserializeNullObject()
        {
            const string json = @"null";

            var serializer = new JsonSerializer<Bar>();

            var instance = serializer.Deserialize(json);

            Assert.That(instance, Is.Null);
        }

        [Test]
        public void CanDeserializeWithEncryptRootObjectEnabled()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var json =
                @""""
                + encryptionMechanism.Encrypt(@"{""Baz"":{""Qux"":""abc"",""Garply"":true},""Corge"":123.45}")
                + @"""";

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
                EncryptRootObject = true
            };

            var serializer = new JsonSerializer<Bar>(configuration);

            var result = serializer.Deserialize(json);

            Assert.That(result.Baz.Garply, Is.EqualTo(true));
            Assert.That(result.Baz.Qux, Is.EqualTo("abc"));
            Assert.That(result.Corge, Is.EqualTo(123.45));
        }

        [Test]
        public void AClassDecoratedWithTheEncryptAttributeIsDecrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var json =
                @""""
                + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                + @"""";

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Grault>(configuration);

            var result = serializer.Deserialize(json);

            Assert.That(result.Garply, Is.EqualTo(true));
            Assert.That(result.Qux, Is.EqualTo("abc"));
        }

        [Test]
        public void APropertyDecoratedWithTheEncryptAttributeIsDecrypted()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var json =
                @"{""Qux"":"""
                + encryptionMechanism.Encrypt(@"""abc""")
                + @""",""Garply"":"""
                + encryptionMechanism.Encrypt("true")
                + @"""}";

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Waldo>(configuration);

            var result = serializer.Deserialize(json);

            Assert.That(result.Garply, Is.EqualTo(true));
            Assert.That(result.Qux, Is.EqualTo("abc"));
        }

        [Test]
        public void DuplicatedEncryptAttributesHaveNoEffectOnDeserialization()
        {
            var encryptionMechanism = new Base64EncryptionMechanism();

            var json =
                @"{""Grault"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @",""Waldo"":"
                    + @""""
                    + encryptionMechanism.Encrypt(@"{""Qux"":""abc"",""Garply"":true}")
                    + @""""
                + @"}";

            var configuration = new JsonSerializerConfiguration
            {
                EncryptionMechanism = encryptionMechanism,
            };

            var serializer = new JsonSerializer<Thud>(configuration);

            var result = serializer.Deserialize(json);

            Assert.That(result.Grault.Qux, Is.EqualTo("abc"));
            Assert.That(result.Grault.Garply, Is.EqualTo(true));
            Assert.That(result.Waldo.Qux, Is.EqualTo("abc"));
            Assert.That(result.Waldo.Garply, Is.EqualTo(true));
        }

        [Test]
        public void IgnoresUnknownMembers()
        {
            var json = @"{""Qux"":""abc"",""Garply"":true,""Unknown"":{""Wat"":""Huh?""}}";

            var serializer = new JsonSerializer<Baz>();

            var result = serializer.Deserialize(json);

            Assert.That(result.Qux, Is.EqualTo("abc"));
            Assert.That(result.Garply, Is.True);
        }

        [Test]
        public void ReadonlyAddableTypeInjectedIntoNonDefaultConstructorIsNotDoubleAdded()
        {
            var serializer = new JsonSerializer<DontDoubleAddMeBro>();

            var json = @"{""Stuff"":[1,2,3]}";

            var thing = serializer.Deserialize(json);

            Assert.That(thing.Stuff.Count, Is.EqualTo(3));
            Assert.That(thing.Stuff, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        public class DontDoubleAddMeBro
        {
            private readonly List<int> _stuff;

            public DontDoubleAddMeBro(List<int> stuff)
            {
                _stuff = stuff;
            }

            public List<int> Stuff
            {
                get { return _stuff; }
            }
        }

        [Test]
        public void CanDeserializeNonDefaultConstructor()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<FooWithNonDefaultConstructor>();

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
            Assert.That(foo.Baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDeserializeNonDefaultConstructorWithExtraProperties()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<FooWithNonDefaultConstructorAndExtraProperty>();

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
            Assert.That(foo.Baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CanDeserializeClassWithMultiplNonDefaultConstructorsWhenOneIsDecoratedWithJsonConstructorAttribute()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<FooWithMultipleNonDefaultConstructorsButOneDecoratedWithJsonConstructorAttribute>();

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
            Assert.That(foo.Baz, Is.EqualTo("abc"));
        }

        [Test]
        public void CannotDeserializeClassWithMultiplNonDefaultConstructors()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<FooWithMultipleNonDefaultConstructors>();

            Assert.That(() => serializer.Deserialize(json), Throws.InstanceOf<XSerializerException>());
        }

        [Test]
        public void CannotDeserializeClassWithMultiplNonDefaultConstructorsWhenMoreThanOneIsDecoratedWithJsonConstructorAttribute()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<FooWithMultipleNonDefaultConstructorsDecoratedWithJsonConstructorAttribute>();

            Assert.That(() => serializer.Deserialize(json), Throws.InstanceOf<XSerializerException>());
        }

        [Test]
        public void CannotDeserializeAbstractClass()
        {
            var json = @"{""Bar"":123,""Baz"":""abc""}";

            var serializer = new JsonSerializer<AbstractFoo>();

            Assert.That(() => serializer.Deserialize(json), Throws.InstanceOf<XSerializerException>());
        }

        [Test]
        public void CanCustomizePropertyName()
        {
            var foo = new FooWithJsonPropertyAttributes { Bar = 123, Baz = "abc" };

            var serializer = new JsonSerializer<FooWithJsonPropertyAttributes>();

            var json = serializer.Serialize(foo);

            Assert.That(json, Is.EqualTo(@"{""qux"":123,""garply"":""abc""}"));

            var roundTrip = serializer.Deserialize(json);

            Assert.That(roundTrip.Bar, Is.EqualTo(foo.Bar));
            Assert.That(roundTrip.Baz, Is.EqualTo(foo.Baz));
        }

        [Test]
        public void CanSerializeInterface()
        {
            var serializer = new JsonSerializer<ISpam>();

            ISpam ham = new Spam { Eggs = 123 };

            var json = serializer.Serialize(ham);

            Assert.That(json, Is.EqualTo(@"{""Eggs"":123}"));
        }

        [Test]
        public void CannotDeserializeInterfaceWhenNoMappingIsSpecified()
        {
            var json = @"{""Eggs"":123}";

            var serializer = new JsonSerializer<ISpam>();

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<XSerializerException>());
        }

        [Test]
        public void CanDeserializeInterfaceWhenMappingIsSpecifiedViaAttribute()
        {
            var json = @"{""Eggs"":123}";

            var serializer = new JsonSerializer<IHam>();

            var ham = serializer.Deserialize(json);

            Assert.That(ham, Is.InstanceOf<Ham>());
            Assert.That(ham.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeInterfaceWhenMappingIsSpecifiedViaConfiguration()
        {
            var json = @"{""Eggs"":123}";

            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(ISpam), typeof(Spam));
            var serializer = new JsonSerializer<ISpam>(configuration);

            var spam = serializer.Deserialize(json);

            Assert.That(spam, Is.InstanceOf<Spam>());
            Assert.That(spam.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeInterfaceWhenMappingIsSpecifiedByBothAttributeAndConfiguration()
        {
            var json = @"{""Eggs"":123}";

            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(IHam), typeof(Bacon));
            var serializer = new JsonSerializer<IHam>(configuration);

            var ham = serializer.Deserialize(json);

            Assert.That(ham, Is.InstanceOf<Bacon>()); // Choose the configured mapping, not the attribute mapping
            Assert.That(ham.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanSerializeInterfacePropertyWithoutSpecifyingMapping()
        {
            var serializer = new JsonSerializer<Breakfast>();

            var food = new Breakfast
            {
                Menu = new Spam
                {
                    Eggs = 123
                }
            };

            var json = serializer.Serialize(food);

            Assert.That(json, Is.EqualTo(@"{""Menu"":{""Eggs"":123}}"));
        }

        [Test]
        public void CanDeserializeInterfacePropertyWhenMappingIsSpecifiedViaAttributedInterface()
        {
            var serializer = new JsonSerializer<Lunch>();

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Ham>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeInterfacePropertyWhenMappingIsSpecifiedViaConfigurationByType()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(ISpam), typeof(Spam));
            var serializer = new JsonSerializer<Breakfast>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Spam>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeInterfacePropertyWhenMappingIsSpecifiedViaAttributedProperty()
        {
            var serializer = new JsonSerializer<Dinner>();

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Spam>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void CanDeserializeInterfacePropertyWhenMappingIsSpecifiedViaConfigurationByProperty()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByProperty.Add(typeof(Breakfast).GetProperty("Menu"), typeof(Spam));
            var serializer = new JsonSerializer<Breakfast>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Spam>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void GivenMultipleMappingsConfigurationByPropertyHasHighestPriority()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByProperty.Add(typeof(FourthMeal).GetProperty("Menu"), typeof(Prosciutto));
            configuration.MappingsByType.Add(typeof(IHam), typeof(ChristmasHam));
            var serializer = new JsonSerializer<FourthMeal>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Prosciutto>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void GivenMultipleMappingsConfigurationByInterfaceHasSecondHighestPriority()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(IHam), typeof(Prosciutto));
            var serializer = new JsonSerializer<FourthMeal>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Prosciutto>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void GivenMultipleMappingsAttributedPropertyHasThirdHighestPriority()
        {
            var serializer = new JsonSerializer<FourthMeal>();

            var json = @"{""Menu"":{""Eggs"":123}}";

            var food = serializer.Deserialize(json);

            Assert.That(food.Menu, Is.InstanceOf<Bacon>());
            Assert.That(food.Menu.Eggs, Is.EqualTo(123));
        }

        [Test]
        public void MismatchedAttributedInterfaceThrowsExceptionOnDeserialization()
        {
            var serializer = new JsonSerializer<IRetch>();

            var json = @"{""Eggs"":123}";

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void MismatchedAttributedInterfacePropertyThrowsExceptionOnDeserialization()
        {
            var serializer = new JsonSerializer<DryHeave>();

            var json = @"{""Menu"":{""Eggs"":123}}";

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void MismatchedPropertyWithAttributedInterfaceTypeThrowsExceptionOnDeserialization()
        {
            var serializer = new JsonSerializer<Vomit>();

            var json = @"{""Menu"":{""Eggs"":123}}";

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void MismatchedConfigurationByTypeThrowsExceptionOnDeserialization()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(IHam), typeof(Spam));
            var serializer = new JsonSerializer<Vomit>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void MismatchedConfigurationByPropertyThrowsExceptionOnDeserialization()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByProperty.Add(typeof(Lunch).GetProperty("Menu"), typeof(Spam));
            var serializer = new JsonSerializer<Vomit>(configuration);

            var json = @"{""Menu"":{""Eggs"":123}}";

            Assert.That(() => serializer.Deserialize(json), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void CanOverrideAbstractClass()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(WidgetBase), typeof(Widget));
            var serializer = new JsonSerializer<WidgetBase>(configuration);

            var json = @"{""Gadget"":123}";

            var widget = serializer.Deserialize(json);

            Assert.That(widget, Is.InstanceOf<Widget>());
            Assert.That(widget.Gadget, Is.EqualTo(123));
        }

        [Test]
        public void CanOverrideConcreteClass()
        {
            var configuration = new JsonSerializerConfiguration();
            configuration.MappingsByType.Add(typeof(Widget), typeof(WidgetDerived));
            var serializer = new JsonSerializer<Widget>(configuration);

            var json = @"{""Gadget"":123}";

            var widget = serializer.Deserialize(json);

            Assert.That(widget, Is.InstanceOf<WidgetDerived>());
            Assert.That(widget.Gadget, Is.EqualTo(123));
        }

        public class FooWithNonDefaultConstructor
        {
            private readonly int _bar;
            private readonly string _baz;

            public FooWithNonDefaultConstructor(int bar, string baz)
            {
                _bar = bar;
                _baz = baz;
            }

            public int Bar { get { return _bar; } }
            public string Baz { get { return _baz; } }
        }

        public class Foo3
        {
            public Foo3()
            {
            }

            public Foo3(int bar, string baz)
            {
                Bar = bar;
                Baz = baz;
            }

            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        public class FooWithNonDefaultConstructorAndExtraProperty
        {
            private readonly string _baz;

            public FooWithNonDefaultConstructorAndExtraProperty(string baz)
            {
                _baz = baz;
            }

            public int Bar { get; set; }
            public string Baz { get { return _baz; } }
        }

        public class FooWithMultipleNonDefaultConstructorsButOneDecoratedWithJsonConstructorAttribute
        {
            [JsonConstructor]
            public FooWithMultipleNonDefaultConstructorsButOneDecoratedWithJsonConstructorAttribute(int bar)
            {
                Bar = bar;
            }

            public FooWithMultipleNonDefaultConstructorsButOneDecoratedWithJsonConstructorAttribute(int bar, string baz)
            {
                Bar = bar;
                Baz = baz;
            }

            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        public class FooWithMultipleNonDefaultConstructors
        {
            public FooWithMultipleNonDefaultConstructors(int bar)
            {
                Bar = bar;
            }

            public FooWithMultipleNonDefaultConstructors(int bar, string baz)
            {
                Bar = bar;
                Baz = baz;
            }

            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        public class FooWithMultipleNonDefaultConstructorsDecoratedWithJsonConstructorAttribute
        {
            [JsonConstructor]
            public FooWithMultipleNonDefaultConstructorsDecoratedWithJsonConstructorAttribute(int bar)
            {
                Bar = bar;
            }

            [JsonConstructor]
            public FooWithMultipleNonDefaultConstructorsDecoratedWithJsonConstructorAttribute(int bar, string baz)
            {
                Bar = bar;
                Baz = baz;
            }

            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        public abstract class AbstractFoo
        {
            public int Bar { get; set; }
            public string Baz { get; set; }
        }

        public static class StaticFoo
        {
            public static int Bar { get; set; }
            public static string Baz { get; set; }
        }

        public class FooWithJsonPropertyAttributes
        {
            [JsonProperty("qux")]
            public int Bar { get; set; }

            [JsonProperty("garply")]
            public string Baz { get; set; }
        }

        public class Bar
        {
            public Baz Baz { get; set; }
            public double Corge { get; set; }
        }

        public class Baz
        {
            public string Qux { get; set; }
            public bool Garply { get; set; }
        }

        [Encrypt]
        public class Grault
        {
            public string Qux { get; set; }
            public bool Garply { get; set; }
        }

        public class Waldo
        {
            [Encrypt]
            public string Qux { get; set; }
            [Encrypt]
            public bool Garply { get; set; }
        }

        public class Fred
        {
            public Baz Baz { get; set; }
            public Grault Grault { get; set; }
            public Waldo Waldo { get; set; }
        }

        public class Thud
        {
            [Encrypt]
            public Grault Grault { get; set; }
            [Encrypt]
            public Waldo Waldo { get; set; }
        }

        [JsonMapping(typeof(Ham))]
        public interface IHam
        {
            int Eggs { get; set; }
        }

        public class Ham : IHam
        {
            public int Eggs { get; set; }
        }

        public class Bacon : IHam
        {
            public int Eggs { get; set; }
        }

        public class Prosciutto : IHam
        {
            public int Eggs { get; set; }
        }

        public class ChristmasHam : IHam
        {
            public int Eggs { get; set; }
        }

        public interface ISpam
        {
            int Eggs { get; set; }
        }

        public class Spam : ISpam
        {
            public int Eggs { get; set; }
        }

        public class Breakfast
        {
            public ISpam Menu { get; set; }
        }

        public class Lunch
        {
            public IHam Menu { get; set; }
        }

        public class Dinner
        {
            [JsonMapping(typeof(Spam))]
            public ISpam Menu { get; set; }
        }

        public class FourthMeal
        {
            [JsonMapping(typeof(Bacon))]
            public IHam Menu { get; set; }
        }

        [JsonMapping(typeof(Spam))]
        public interface IRetch
        {
            int Eggs { get; set; }
        }

        public class Vomit
        {
            public IRetch Menu { get; set; }
        }

        public class DryHeave
        {
            [JsonMapping(typeof(Spam))]
            public IHam Menu { get; set; }
        }

        public abstract class WidgetBase
        {
            public int Gadget { get; set; }
        }

        public class Widget : WidgetBase
        {
        }

        public class WidgetDerived : Widget
        {
        }

        [Test]
        public void CanDeserializeReadonlyPropertyWithNameSpecifiedByJsonPropertyAttribute()
        {
            var serializer = new JsonSerializer<Broken.Foo>();

            var json = @"{""bar"":123}";

            var foo = serializer.Deserialize(json);

            Assert.That(foo.Bar, Is.EqualTo(123));
        }

        public static class Broken
        {
            public class Foo
            {
                private readonly int _bar;

                public Foo(int bar)
                {
                    _bar = bar;
                }

                [JsonProperty("bar")]
                public int Bar { get { return _bar; } }
            }
        }
    }
}
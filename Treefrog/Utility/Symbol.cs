using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Utility
{
    public class Symbol
    {
        internal Symbol (int id, string name, SymbolPool pool)
        {
            Id = id;
            Name = name;
            Pool = pool;
        }

        protected Symbol (Symbol prototype)
            : this(prototype.Id, prototype.Name, prototype.Pool)
        { }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public SymbolPool Pool { get; private set; }

        public static implicit operator int (Symbol symbol)
        {
            return symbol.Id;
        }
    }

    public class SymbolPool
    {
        private static int _nextPoolId = 1;

        private readonly int _poolId;
        private int _nextSymbolId = 1;
        //private Dictionary<int, Symbol> _map;

        public SymbolPool ()
            : this(_nextPoolId++)
        { }

        private SymbolPool (int poolId)
        {
            _poolId = poolId;
            //_map = new Dictionary<int, Symbol>();
        }

        public Symbol GenerateSymbol ()
        {
            return GenerateSymbol("Symbol" + _nextSymbolId);
        }

        public Symbol GenerateSymbol (string name)
        {
            return new Symbol(TakeNextSymbolId(), name, this);
        }

        protected int TakeNextSymbolId ()
        {
            return _nextSymbolId++;
        }
    }

    public class SymbolPool<T> : SymbolPool
        where T : Symbol
    {
        public new T GenerateSymbol ()
        {
            return Activator.CreateInstance(typeof(T), base.GenerateSymbol()) as T;
        }

        public new T GenerateSymbol (string name)
        {
            return Activator.CreateInstance(typeof(T), base.GenerateSymbol(name)) as T;
        }
    }
}

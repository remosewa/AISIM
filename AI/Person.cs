using System;
using System.Collections.Generic;

namespace AI
{
    class Person
    {
        private int hunger = 30;
        private float strength = 50;
        private int health = 100;
        private int age;
        private int wealth = 0;
        private HashSet<Person> friends = new HashSet<Person>();
        private HashSet<Person> family = new HashSet<Person>();
        private List<Action> tendency = new List<Action>();
        private Dictionary<Action, int> tendencyMap = new Dictionary<Action, int>();

        private int actionRemaining = 0;
        private int reproduceRemaining = 0;

        private static Random rnd = new Random();

        public Person(int age, Person parent1, Person parent2) 
        {
            if (age == 0)
            {
                GenerateTendencies(false);
            }
            else
            {
                GenerateTendencies(true);
            }
            if (parent1 != null)
            {
                family.Add(parent1);
                AddFamily(parent1.GetFamily());
            }
            if (parent2 != null)
            {
                family.Add(parent2);
                AddFamily(parent2.GetFamily());
            }
        }


        public Person() : this(0, null, null)
        {

        }

        public HashSet<Person> GetFamily()
        {
            return family;
        }

        private void AddFamily(HashSet<Person> familyToBe)
        {
            foreach (var familyMemberToBe in familyToBe)
            {
                familyMemberToBe.GetFamily().Add(this);
                family.Add(familyMemberToBe);
            }
        }


        private void GenerateTendencies(bool allowKill)
        {
            foreach (Action action in Enum.GetValues(typeof(Action)))
            {
                if (action == Action.KILL && !allowKill)
                {
                    tendencyMap[action] = 0;
                    continue;
                }
                int lowerBounds = 0;
                //required actions.
                if (action == Action.EAT || action == Action.SLEEP)
                {
                    lowerBounds = 1;
                }
                int count = rnd.Next(lowerBounds, 11);
                for (var i = 0; i < count; i += 1)
                {
                    tendency.Add(action);
                }
                tendencyMap[action] = count;

            }
            Shuffle(tendency);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void Tick()
        {
            Action? action = DecideAction();
            if (action == null)
                return;
            switch (action)
            {
                case Action.EAT:
                    Eat();
                    break;
                case Action.SLEEP:
                    Sleep();
                    break;
                case Action.WORKOUT:
                    Workout();
                    break;
                case Action.KILL:
                    Kill();
                    break;
                case Action.COMMUNICATE:
                    Communicate();
                    break;
                case Action.MAKEFRIENDS:
                    MakeFriends();
                    break;
                case Action.REPRODUCE:
                    Reproduce();
                    break;
                case Action.HELP:
                    Help();
                    break;
                case Action.WORK:
                    Work();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Help()
        {
            throw new NotImplementedException();
        }

        private void Work()
        {
            throw new NotImplementedException();
        }

        private void Reproduce()
        {
            throw new NotImplementedException();
        }

        private void MakeFriends()
        {
            throw new NotImplementedException();
        }

        private void Communicate()
        {
            throw new NotImplementedException();
        }

        private void Kill()
        {
            throw new NotImplementedException();
        }

        private void Workout()
        {
            throw new NotImplementedException();
        }

        private void Sleep()
        {
            throw new NotImplementedException();
        }

        private void Eat()
        {
            throw new NotImplementedException();
        }

        private Action? DecideAction()
        {
            if (actionRemaining > 0)
            {
                actionRemaining -= 1;
                return null;
            }
            if (hunger == 100)
            {
                return Action.EAT;
            }
            return tendency[rnd.Next(tendency.Count)];
        }
    }
}

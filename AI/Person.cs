using System;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    public class Person
    {
        private int hunger = 30;
        private double strength = 50;
        private double health = 100;
        private int age;
        private double wealth = 0;
        private double skill = 0;
        private int awakeness = 16;
        private Gender gender;

        private int maxHealth = 100;
        private double maxStrength = 100;
        private HashSet<Person> friends = new HashSet<Person>();
        private HashSet<Person> family = new HashSet<Person>();
        private List<Action> tendency = new List<Action>();
        private Dictionary<Action, int> tendencyMap = new Dictionary<Action, int>();

        private int actionRemaining = 0;
        private int reproduceRemaining = 0;
        private long ticks = 0;
        private Dictionary<Person, int> helpReceivedMap = new Dictionary<Person, int>();
        


        private Person otherParent = null;

        private static Random rnd = new Random();
        private Controller controller;

        public Person(int age, Person parent1, Person parent2, Controller controller)
        {
            this.controller = controller;
            this.age = age;
            int randomNum = rnd.Next(2);
            if (randomNum == 0)
            {
                gender = Gender.Male;
            }
            else
            {
                gender = Gender.Female;
            }
            
            if (parent1 != null && parent2 != null)
            {  
                GenerateTendencies(parent1, parent2);
            }
            else
            {
                GenerateTendencies();
                //TODO: randomize stats
            }
            if (parent1 != null)
            {
                parent1.GetFamily().Add(this);
                family.Add(parent1);
                AddFamily(parent1.GetFamily());
            }
            if (parent2 != null)
            {
                parent2.GetFamily().Add(this);
                family.Add(parent2);
                AddFamily(parent2.GetFamily());
            }
        }


        public Person(Controller controller) : this(0, null, null, controller)
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


        //Random
        private void GenerateTendencies()
        {
            foreach (Action action in Enum.GetValues(typeof(Action)))
            {

                int lowerBounds = 0;
                //required actions.
                if (action == Action.EAT || action == Action.SLEEP)
                {
                    lowerBounds = 1;
                }
                
                int count = rnd.Next(lowerBounds, 11);
                AddTendency(action, count);
            }
            Shuffle(tendency);
        }

        //instead of creating tendencies randomly, generated them from the average of the two parents.
        private void GenerateTendencies(Person parent1, Person parent2)
        {
            foreach (Action action in Enum.GetValues(typeof(Action)))
            {
                var avg = (parent1.tendencyMap[action] + parent2.tendencyMap[action])/2;
                AddTendency(action, avg);
            }
            Shuffle(tendency);
        }

        private static void Shuffle<T>(IList<T> list)
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
            ComputeAgeStuff();
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
                case Action.STEAL:
                    Steal();
                    break;
                case Action.STUDY:
                    Study();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ValidateStats();
        }

        private void Steal()
        {
            var wealthyList = new List<Person>();
            wealthyList.AddRange(family);
            wealthyList.AddRange(friends);
            wealthyList.Sort(delegate(Person a, Person b)
            {
                if (a.wealth > b.wealth)
                {
                    return -1;
                }
                if (a.wealth < b.wealth)
                {
                    return 1;
                }
                return 0;
            });
            var topList = wealthyList.Take(10).ToList();
            topList.Sort(delegate(Person a, Person b)
            {
                var helpReceivedFromA = helpReceivedMap.ContainsKey(a) ? helpReceivedMap[a] : 0;
                var helpReceivedFromB = helpReceivedMap.ContainsKey(b) ? helpReceivedMap[b] : 0;

                if (helpReceivedFromA > helpReceivedFromB)
                {
                    return 1;
                }
                if (helpReceivedFromA < helpReceivedFromB)
                {
                    return -1;
                }
                return 0;
            });
            var personToStealFrom = topList.Last();
            for (int i = 0; i < 9; i += 1)
            {
                if (rnd.Next(10) < 2) //20% chance to steal from this person.
                {
                    personToStealFrom = topList[i];
                    break;
                }
            }
            if (personToStealFrom.wealth > 0)
            {
                actionRemaining = 2; //three hours to steal. current hour, + 2 more
                var tempSkill = skill > 100 ? 100 : skill;
                var otherSkill = personToStealFrom.skill > 100 ? 100 : personToStealFrom.skill;
                tempSkill = tempSkill + rnd.Next(-20, 21);
                var odds = tempSkill - otherSkill;
                bool isCaught = rnd.Next(100) > odds;
                int wealthToSteal = (int)((rnd.Next(10, 91) / 100.00) * personToStealFrom.wealth); //steal 10 - 90% of their wealth
                if (isCaught) //if you're caught, lose a third of your booty
                {
                    wealthToSteal = wealthToSteal/3;
                    personToStealFrom.health -= rnd.Next(0, (int)strength);
                    health -= rnd.Next(0, (int) personToStealFrom.strength);
                    if (personToStealFrom.health < 0)
                    {
                        controller.kill(personToStealFrom, this);
                    }
                    if (health < 0)
                    {
                        controller.die(this);
                    }
                    if (rnd.Next(100) > 50 && tendency.Contains(Action.STEAL)) //50 % chance if they are unsuccessful they might remove this as a tendency.
                    {
                        if (health < 10)//if health drops below 10% then we will never steal again.
                        {
                            tendency.RemoveAll(delegate(Action aciton)
                            {
                                return aciton == Action.STEAL;
                            });
                        }
                        else
                        {
                            tendency.Remove(Action.STEAL);
                        }
                        
                        tendencyMap[Action.STEAL] -= 1;
                    }

                }
                else
                {
                    if (rnd.Next(100) > 50) //50 % chance if they are successful they might add this as a tendency.
                    {
                        tendency.Add(Action.STEAL);
                        tendencyMap[Action.STEAL] += 1;
                    }
                }
                personToStealFrom.wealth -= wealthToSteal;
                wealth += wealthToSteal;
                if( skill < 100) skill += 1; //learned a little how to steal.
            }
            

        }

        private void Study()
        {
            skill += 1;
            actionRemaining = 1;
        }

        private void ValidateStats()
        {
            throw new NotImplementedException();
        }

        private void EnableTendency(Action action)
        {
            if (tendencyMap[action] < 0)
            {
                int count = tendencyMap[action]*-1;
                AddTendency(action, count);
            }
            else
            {
                throw new ArgumentException("action " + action + " is already enabled.");
            }
        }

        private void AddTendency(Action action, int count)
        {
            if (tendency.Contains(action))
            {
                throw new ArgumentException("action " + action + " has already been added.");
            }
            int neg = 1;
            if (age < 5 && !(new[] { Action.EAT, Action.SLEEP }).Contains(action))
            {
                neg = -1;
            }
            else if (age < 16 && !(new[] { Action.EAT, Action.SLEEP, Action.COMMUNICATE, Action.MAKEFRIENDS, Action.STUDY, }).Contains(action))
            {
                neg = -1;
            }
            if (neg == 1)
            {
                for (var i = 0; i < count; i += 1)
                {
                    tendency.Add(action);
                }
            }
            tendencyMap[action] = count * neg;
            
        }

        private void ComputeAgeStuff()
        {
            ticks += 1;
            //one year
            if (ticks%8760 == 0)
            {
                age += 1;
                if (age == 16)
                {
                    EnableTendency(Action.REPRODUCE);
                    EnableTendency(Action.WORKOUT);
                    EnableTendency(Action.WORK);
                    EnableTendency(Action.HELP);
                    EnableTendency(Action.KILL);
                    EnableTendency(Action.STEAL);
                }
                if (age == 5)
                {
                    EnableTendency(Action.COMMUNICATE);
                    EnableTendency(Action.MAKEFRIENDS);
                    EnableTendency(Action.STUDY);
                }
                Shuffle(tendency);
            }
            if (age > 16)
            {
                wealth -= 0.041667; //cost of living.
            }
            
            if (reproduceRemaining > 0)
            {
                reproduceRemaining -= 1;
                if (reproduceRemaining == 0)
                {
                    new Person(0, this, otherParent, controller);
                    strength += maxStrength/10;
                }
                else
                {
                    //hourly strength reduction due to reproduction.
                    strength -= 0.012;
                }
            }
            if (age > 20)
            {
                maxHealth = 100 - (age - 20);
                maxStrength = 100 - (age - 20);
            }
            if (age > 50 && skill > 0)
            {
                skill = skill/1.01;
            }
            //lose strength when not working out
            if (strength > 50)
            {
                strength -= 0.04167;
            }
            if (awakeness <= 0)
            {
                health -= 0.5;
            }
            awakeness -= 1;
            hunger += 3;
            if (hunger >= 100)
            {
                health -= 2;
            }
            if (health == 0 || rnd.Next(7000000) == 0)
            {
                controller.die(this);
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
            controller.findMate(this, ticks);
            actionRemaining = 2;
        }

        public void Reproduce(Person otherPerson)
        {
            if (otherPerson != null && reproduceRemaining == 0 && gender == Gender.Female)
            {
                var reproChance = (health + strength + otherPerson.health + otherPerson.strength)/4;
                if (reproChance > rnd.Next(100))
                {
                    otherParent = otherPerson;
                    reproduceRemaining = 6480; // 9 mo  
                }
            }
            hunger += 2;
            strength += 1;
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
            actionRemaining = 1;
            health += 2;
            strength += 2;
            hunger += 2;
        }

        private void Sleep()
        {
            int sleepTime = rnd.Next(5, 11);
            actionRemaining = sleepTime;
            awakeness += sleepTime * 2;
        }

        private void Eat()
        {
            //penalty for over eating.
            if (hunger < 20)
            {
                health -= 1;
            }
            hunger = 0;
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
            //70% chance that you'll sleep if you are super tired.
            if (awakeness <= 0 & rnd.Next(10) < 7)
            {
                return Action.SLEEP;
            }
            
            return tendency[rnd.Next(tendency.Count)];
        }
    }
}

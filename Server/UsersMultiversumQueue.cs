using CBC.Shared;
using ConcurrentLinkedList;
using System.Collections.Concurrent;

namespace CBC.Server
{
    internal class UsersMultiversumQueue
    {
        internal UsersMultiversumQueue(int AgeMin, int AgeMax)
        {
            var Fields = AgeMax - AgeMin;
            Males = new LinkedList<UserPreferences>[Fields];
            Females = new LinkedList<UserPreferences>[Fields];
            for (int i = 0; i < Fields; i++)
            {
                Males[i] = new LinkedList<UserPreferences>();
                Females[i] = new LinkedList<UserPreferences>();
            }
        }

        internal void Push(int Age, bool Female, UserPreferences user)
        {
            if(Female)
            {
                Females[Age].AddLast(user);
                return;
            }
            Males[Age].AddLast(user);
            //Females[Age].Enqueue(user);
        }

        internal string GetId(UserPreferences requirements, QueueUser user)
        {
            Console.WriteLine( "w males jes "+Males[24].Count);
            try { 
                Console.WriteLine($"{Males[24].First.Value.ConnectionId}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
            int avr = (requirements.MinAge + requirements.MaxAge) / 2;
            string ConnID = null;
            for (int i = requirements.MaxAge; i > requirements.MinAge; i--)
            {
                //if (requirements.AcceptMale)
                //{
                //    if (r < requirements.MaxAge && Males[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Males[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                //if (requirements.AcceptFemale)
                //{
                //    if (r < requirements.MaxAge && Females[r].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //    if (l >= 0 && Females[l].GetFirstUserWithCondition(ueueUser => ueueUser.MinAge > user.Age, ref ConnID))
                //    {
                //        break;
                //    }
                //}
                if (requirements.AcceptFemale)
                {
                    Console.WriteLine($"sprawdziam{Females[i]}");
                    if (Females[i].GetFirstUserWithCondition(ueueUser => 
                    ueueUser.MinAge < user.Age &&
                    ueueUser.MaxAge > user.Age &&
                    user.IsFemale? ueueUser.AcceptFemale: ueueUser.AcceptMale
                    , ref ConnID))
                    {
                        break;
                    }
                }
                if (requirements.AcceptMale) {
                    Console.WriteLine($"sprawdziam{Males[i]}");
                    if (Males[i].GetFirstUserWithCondition(ueueUser =>
                    ueueUser.MinAge < user.Age &&
                    ueueUser.MaxAge > user.Age &&
                    user.IsFemale ? ueueUser.AcceptFemale : ueueUser.AcceptMale
                    , ref ConnID))
                    {
                        break;
                    }
                }
            }
            Console.WriteLine("nie znalaz"+ requirements.AcceptFemale+ requirements.AcceptFemale);
            return ConnID;
        }

        LinkedList<UserPreferences>[] Males;
        LinkedList<UserPreferences>[] Females;
        //todo make concurent linked list with pointer to both ends
    }


    internal static class ConcurrentQueueExtention
    {
        public static bool GetFirstUserWithCondition(this LinkedList<UserPreferences> que, Func<UserPreferences, bool> condition,ref string ConnId)
        {
            //TODO make it concurrent
            try { 
                LinkedListNode<UserPreferences>? node = que.First; // Start with the first node
                while (node != null)
                {
                    var nextNode = node.Next; // Save the next node
                    if (condition(node.Value))
                    {
                        que.Remove(node); // Remove the current node
                        ConnId = node.Value.ConnectionId; // Save the connection id
                        return true;
                    }
                    node = nextNode; // Move to the next node
                }
            }
            catch (Exception e) { 
                return false;
            }
            return false;

        }
    }
}

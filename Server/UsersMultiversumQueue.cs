using CBC.Shared;
using ConcurrentLinkedListQueue;
using System.Collections.Concurrent;

namespace CBC.Server
{
    internal class UsersMultiversumQueue
    {
        internal UsersMultiversumQueue(int AgeMin, int AgeMax)
        {
            var Fields = AgeMax - AgeMin;
            Males = new ConcurrentLinkedListQueueUserPreferences[Fields];
            Females = new ConcurrentLinkedListQueueUserPreferences[Fields];
            for (int i = 0; i < Fields; i++)
            {
                Males[i] = new ConcurrentLinkedListQueueUserPreferences();
                Females[i] = new ConcurrentLinkedListQueueUserPreferences();
            }
        }

        internal void Push(int Age, bool Female, UserPreferences user)
        {
            if(Female)
            {
                Females[Age].Enqueue(user);
                return;
            }
            Males[Age].Enqueue(user);
            Console.WriteLine(Males[Age].Count() + "malesow");
            //Females[Age].Enqueue(user);
        }

        internal string GetId(UserPreferences requirements, QueueUser user, string ConnID)
        {
            int avr = (requirements.MinAge + requirements.MaxAge) / 2;
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
                    if (Females[i].GetFirstUserWithCondition(ueueUser => 
                    ueueUser.MinAge < user.Age &&
                    ueueUser.MaxAge > user.Age &&
                    (user.IsFemale? ueueUser.AcceptFemale: ueueUser.AcceptMale) &&
                    ueueUser.ConnectionId != ConnID
                    , ref ConnID))
                    {
                        return ConnID;
                    }
                }
                if (requirements.AcceptMale) {
                    if (Males[i].GetFirstUserWithCondition(ueueUser =>
                    ueueUser.MinAge < user.Age &&
                    ueueUser.MaxAge > user.Age &&
                    (user.IsFemale ? ueueUser.AcceptFemale: ueueUser.AcceptMale) &&
                    ueueUser.ConnectionId != ConnID
                    , ref ConnID))
                    {
                        return ConnID;
                    }
                    Console.WriteLine(Males[i].Count()+  "  totot");
                }
            }
            return null;
        }

        internal string RemoveUser(QueueUser user, string ConnID)
        {
            if (user.IsFemale) {
                if (Females[user.Age].GetFirstUserWithCondition(ueueUser =>
                (ueueUser.ConnectionId == ConnID)
                        , ref ConnID))
                    {
                        return ConnID;
                    }
                }
            return null;
        }


        internal ConcurrentLinkedListQueueUserPreferences[] Males;
        internal ConcurrentLinkedListQueueUserPreferences[] Females;
        //todo make concurent linked list with pointer to both ends
    }


    //internal static class ConcurrentQueueExtention
    //{
    //    public static bool GetFirstUserWithCondition(this LinkedList<UserPreferences> que, Func<UserPreferences, bool> condition,ref string ConnId)
    //    {
    //        try { 
    //            LinkedListNode<UserPreferences>? node = que.First; // Start with the first node
    //            while (node is object)
    //            {
    //                if (condition(node.Value))
    //                {
    //                    que.Remove(node); // Remove the current node
    //                    ConnId = node.Value.ConnectionId; // Save the connection id
    //                    return true;
    //                }
    //                node = node.Next; // Save the next node
    //            }
    //        }
    //        catch (Exception e) { 
    //            return false;
    //        }
    //        return false;

    //    }
    //}
}

namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {      
        List<(string source, string destination)> segment;
        float cost;

        public Tour(List<(string source, string destination)> segment, float cost)
        {
            this.segment = segment;
            this.cost = cost;
        }
        public Tour()
        {
            this.segment = new List<(string source, string destination)>();
            this.cost = 0.0f ;
        }
        // propriétés

        // Coût total de la tournée
        public float Cost
        {
            get { return this.cost; }   
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return this.segment.Count; }   
        }


        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            return this.segment.Contains(segment); //Retourne vraie si la liste segment contient le segment, retourne faux sinon
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            Console.WriteLine($"Coût total de la tournée : {this.cost}");
            Console.WriteLine("Trajets :");
            foreach ((string source, string destination) segment in this.segment)
            {
                Console.WriteLine($"{segment.source} -> {segment.destination}");
            }
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }
}

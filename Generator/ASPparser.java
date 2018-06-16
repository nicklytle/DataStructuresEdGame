
import java.io.File;
import java.util.Scanner;
import java.io.FileNotFoundException;


public class ASPparser{

public static void main(String[] args){
        if(args.length > 0) {
            File file = new File(args[0]);
			
			try{
			Scanner scanner = new Scanner(file);
			
			String version = scanner.nextLine();
			String fileToRead = scanner.nextLine();
			String errorMessageorSolving = scanner.nextLine();
			String satisfiability = scanner.nextLine();
			while(scanner.hasNextLine()){
				//System.out.println(scanner.nextLine());
			}
			
			scanner.close();
			}
			catch (FileNotFoundException e) {
				e.printStackTrace();
			}
            
        }
}

}
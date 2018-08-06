import java.io.*;
import java.util.*;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;


public class selectUser{
	int desiredUserId;
	String desiredLvlFileName;
	String outputFile;
	public selectUser(){
		desiredUserId = 0;
		desiredLvlFileName = "";
		outputFile = "";
	}
	public void readFromFile() {
		File file = null;
		PrintStream print = null;
		try {
			//file = new File("resultingData.csv");
			file = new File(outputFile);
			print = new PrintStream(file);
		} catch (IOException e) {
			e.printStackTrace();
			System.exit(0);
		}
		Scanner fp = null;
		try{
			fp = new Scanner(new File("actiontable_July16_ResultsCopy.csv"));
		}
		catch(FileNotFoundException e){
			System.exit(0);
		}
		while(fp.hasNextLine()) {
			String line = fp.nextLine();
			if(!(Character.isDigit(line.charAt(0)))){
				System.exit(0);
			}
			//totalLineCt++;
			//System.out.println(line);
			Scanner linescan = new Scanner(line);
			linescan.useDelimiter(",");
			int userId = 0;
			int logId = 0;
			String levelName = "";
			try{
				logId = linescan.nextInt();
				userId = linescan.nextInt();
				levelName = linescan.next();
			}
			catch(InputMismatchException e){
				System.out.println(e);
				System.exit(0);
			}
			if((userId == desiredUserId) && (levelName.equals(desiredLvlFileName))){
				//System.out.println(line);
				print.println(line);
				//linesICareAboutCt++;
			}
			linescan.close();
		}
		fp.close();
	}
	public static void main(String[] args){
		selectUser current = new selectUser();
		current.desiredUserId = Integer.parseInt(args[0]);
		current.desiredLvlFileName = args[1];
		current.outputFile = args[2];
		current.readFromFile();
	}
}
package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"
	"syscall"
	"unicode"

	"github.com/atotto/clipboard"
	"github.com/fatih/color"
	lastpass "github.com/mattn/lastpass-go"
	"golang.org/x/crypto/ssh/terminal"
)

func printHeader() {
	fmt.Print("  _               _  ")
	color.Red(" _____                              ")
	fmt.Print(" | |             | | ")
	color.Red("|  __ \\                           _ ")
	fmt.Print(" | |     __ _ ___| |_")
	color.Red("| |__) |_ _ ___ ___              | |")
	fmt.Print(" | |    / _` / __| __")
	color.Red("|  ___/ _` / __/ __|  _   _   _  | |")
	fmt.Print(" | |___| (_| \\__ \\ |_")
	color.Red("| |  | (_| \\__ \\__ \\ (_) (_) (_) | |")
	fmt.Print(" |______\\__,_|___/\\__")
	color.Red("|_|   \\__,_|___/___/             |_|")

	fmt.Println()
}

func readLine() (string, error) {
	reader := bufio.NewReader(os.Stdin)

	input, err := reader.ReadString('\n')

	if err != nil {
		return "", err
	}

	input = strings.TrimSpace(input)

	return input, nil
}

func readCredentials() (email, password string, err error) {
	fmt.Print("Email: ")

	email, err = readLine()

	if err != nil {
		return "", "", err
	}

	fmt.Print("Password: ")

	bytePassword, err := terminal.ReadPassword(int(syscall.Stdin))

	if err != nil {
		return "", "", err
	}

	return email, string(bytePassword), nil
}

func matchAccount(acc *lastpass.Account, query string) bool {
	return strings.Contains(strings.ToLower(acc.Name), strings.ToLower(query)) ||
		strings.Contains(strings.ToLower(acc.Username), strings.ToLower(query)) ||
		strings.Contains(strings.ToLower(acc.Url), strings.ToLower(query))
}

func printPassword(password string) {
	for _, runeValue := range password {
		if unicode.IsLetter(runeValue) {
			fmt.Print(string(runeValue))
		} else if unicode.IsDigit(runeValue) {
			color.New(color.FgRed).Print(string(runeValue))
		} else {
			color.New(color.FgBlue).Print(string(runeValue))
		}
	}

	fmt.Println()
}

func main() {
	printHeader()

	email, password, err := readCredentials()

	vault, err := lastpass.CreateVault(email, password)

	fmt.Println()

	if err != nil {
		fmt.Println("We could not log you in: ", err.Error())
		return
	}

	fmt.Printf("Found %d accounts.\n", len(vault.Accounts))

	fmt.Println("Search accounts or quit:")

	for query, _ := readLine(); query != "q" && query != "quit"; {
		matchedAccounts := make([]*lastpass.Account, 0)

		i := 1
		for _, acc := range vault.Accounts {
			if matchAccount(acc, query) {
				matchedAccounts = append(matchedAccounts, acc)

				fmt.Printf("[%d] %s: %s\n", i, acc.Name, acc.Username)
				i++
			}
		}

		fmt.Println("Select account: ")
		var accIndex int
		_, err = fmt.Scanf("%d\n", &accIndex)
		if err == nil && accIndex >= 1 && accIndex <= len(matchedAccounts) {
			accPassword := matchedAccounts[accIndex-1].Password

			fmt.Print("Copy to clipboard or show to console? ")
			copyToClipboard, _ := readLine()
			copyToClipboard = strings.ToLower(copyToClipboard)
			if copyToClipboard[0] == 'c' {
				clipboard.WriteAll(accPassword)
				fmt.Println("Copied to clipboard.")
			} else {
				printPassword(accPassword)
			}
		}

		fmt.Println("Search accounts or quit:")
		query, _ = readLine()
	}
}

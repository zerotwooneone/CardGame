# CardGame.Web/ClientApp (Angular Frontend)

## Purpose of the Project

This project contains the Angular single-page application (SPA) that serves as the user interface for the Love Letter card game. It is responsible for:
*   Rendering the game state and user interactions.
*   Communicating with the backend API (`CardGame.Web`) to send player actions and receive game updates.
*   Managing client-side state, navigation, and user experience.

## Code Guidelines

### What Should Be in This Project:
*   **Angular Components**: Reusable UI building blocks (e.g., `CardDisplayComponent`, `GameViewComponent`).
*   **Angular Services**: For encapsulating business logic, API communication (`GameStateService`, `DeckService`), and shared state.
*   **Angular Modules**: For organizing the application into cohesive blocks of functionality.
*   **HTML Templates**: For defining the structure and layout of components.
*   **SCSS/CSS Stylesheets**: For styling components and the overall application.
*   **TypeScript Code**: For component logic, services, models/interfaces specific to the frontend.
*   **Routing Configuration**: For defining navigation paths within the SPA.
*   **Assets**: Images, fonts, and other static resources used by the frontend.
*   **Environment Configuration**: Files for different build environments (e.g., development, production).

### What Should NOT Be in This Project:
*   **Backend Server Logic**: No C# code, API controllers, or database interactions. This belongs in the .NET projects.
*   **Direct DOM Manipulation (outside of Angular's mechanisms)**: Leverage Angular's data binding and rendering.

## Development Environment Setup

*   **Node.js and npm**: Install Node.js (LTS version recommended, which includes npm) from [nodejs.org](https://nodejs.org/).
*   **Angular CLI**: Install globally:
    ```bash
    npm install -g @angular/cli
    ```
    (Verify version compatibility with the project's `angular.json` or `package.json` if issues arise).
*   **IDE**: Visual Studio Code (recommended for Angular development) or other IDEs with good TypeScript/Angular support.
*   **Install Dependencies**:
    *   Navigate to the `CardGame.Web/ClientApp` directory in your terminal.
    *   Run:
        ```bash
        npm install
        ```
*   **Running the Frontend Development Server**:
    *   Ensure you are in the `CardGame.Web/ClientApp` directory.
    *   Run:
        ```bash
        ng serve
        ```
        Or, if you have an npm script for it in `package.json` (e.g., `npm start`).
    *   This will typically start the Angular development server on `http://localhost:4200/`.
    *   The application will automatically reload if you change any of the source files.
*   **API Proxying**:
    *   The Angular development server uses `proxy.conf.js` (in the `ClientApp` root) to forward API requests (e.g., to `/api/...`) to the backend ASP.NET Core server.
    *   **Ensure the backend server (`CardGame.Web`) is running** for the frontend to successfully make API calls.
*   **Building for Production**:
    ```bash
    ng build --configuration production
    ```
    This will output build artifacts to the `dist/` folder (or as configured in `angular.json`). The `CardGame.Web` project is typically configured to serve these static files when running in production mode.

## Tips for AI Assistants

*   **Angular Project**: This is a standard Angular CLI project. Be familiar with Angular concepts: Components, Services, Modules, Templates, Data Binding, Dependency Injection, RxJS, and Signals.
*   **Key Files/Folders**:
    *   `src/app/features/`: Contains feature modules (e.g., `game`, `auth`). This is where most of the custom code resides.
    *   `src/app/core/`: Contains core services and models used across features (e.g., `AuthService`, `SignalrService`).
    *   `src/app/shared/`: Contains shared components, directives, pipes (though much has been moved to feature-specific folders).
    *   `src/app/app.module.ts`, `src/app/app.component.ts`, `src/app/app-routing.module.ts`: Core application setup.
    *   `angular.json`: Angular CLI project configuration.
    *   `proxy.conf.js`: **CRITICAL** for local development API proxying to the .NET backend. Case-sensitive! (Memory: `f726ee94-28be-4d61-b252-96cf74e95bc8`)
    *   `src/environments/`: Environment-specific settings.
*   **State Management**: Primarily uses Angular Signals and RxJS BehaviorSubjects/Observables within services.
*   **API Communication**: Services use Angular's `HttpClient` to interact with the backend API. Check service methods that make HTTP calls.
*   **Path Aliases**: TypeScript path aliases (e.g., `@core`, `@features`, `@gameComponents`) are used in `tsconfig.json`. Be mindful of these when resolving imports. (Memory: `9f2f3a5c-7c4a-430c-a9d3-635da1e96a0d`)
*   **Styling**: SCSS is used. Look for global styles in `src/styles.scss` and component-specific styles.
*   **Refactoring History**: This project has undergone significant refactoring (see memories, e.g., `112d1490-269b-4a1c-99ac-bb2b92df4a23`, `6f73347e-2a20-4235-89b3-387543cda40a`). Old patterns or file locations might be mentioned in older contexts but may no longer be accurate. **Prioritize recent memories.**
*   **`grep_search` for `.spec.ts` files**: This tool has been unreliable for spec files in this project. Use `list_dir` if `grep_search` fails to find a spec file. (Memory: `55b86031-2a49-4fa8-8aaa-73e6734b3f69`)
*   **Angular Material**: The project uses Angular Material components. Be aware of their specific usage patterns and potential issues (e.g., `mat-form-field` structure - Memory: `80242612-73ff-46ae-a081-d555b70349e1`; ID collision - Memory: `25bc2c5c-0a2f-4fb8-b30d-902d24e0332b`).
